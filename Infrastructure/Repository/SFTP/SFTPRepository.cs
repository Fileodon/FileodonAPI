using System.Text;
using Infrastructure.Interface;
using Infrastructure.Interface.Repository.SFTP;
using Infrastructure.Models;
using Infrastructure.Models.DTO.SFTPAction;
using Infrastructure.Worker;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Infrastructure.Repository.SFTP;

public sealed class SFTPRepository : ISFTPRepository
{
    private readonly ICryptoService _cryptoService;
    private readonly ISFTPUploadJobStore _jobStore;
    private readonly ILogger<SFTPRepository> _logger;

    public SFTPRepository(
        ICryptoService cryptoService,
        ISFTPUploadJobStore jobStore,
        ILogger<SFTPRepository> logger)
    {
        _cryptoService = cryptoService;
        _jobStore = jobStore;
        _logger = logger;
    }

    public async Task<SFTPActionExistsResult> FileExistsAsync(SFTPConnection connection, string remoteFilePath, CancellationToken cancellationToken)
    {
        SftpClient client = CreateSftpClient(connection);

        try
        {
            await client.ConnectAsync(CancellationToken.None);
            bool fileExists = await client.ExistsAsync(remoteFilePath, cancellationToken);
            if (fileExists)
            {
                SftpFileAttributes fileAttributes = await client.GetAttributesAsync(remoteFilePath, cancellationToken);
                return new SFTPActionExistsResult(StatusCode.Ok, FileAttributes: fileAttributes);
            }
            else
            {
                return new SFTPActionExistsResult(StatusCode.NoSuchFile);
            }
        }
        catch (SftpException ex)
        {
            switch (ex.StatusCode)
            {
                case StatusCode.NoSuchFile:
                    return new SFTPActionExistsResult(StatusCode.NoSuchFile, "The specified file does not exist on the SFTP server.");
                case StatusCode.PermissionDenied:
                    return new SFTPActionExistsResult(StatusCode.PermissionDenied, "Permission denied when checking if the file exists on the SFTP server.");
                default:
                    _logger.LogError(ex, "An unexpected SFTP error occurred while checking if the file exists on the SFTP server.");
                    return new SFTPActionExistsResult(StatusCode.Failure, $"An unexpected SFTP error occurred: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while checking if the file exists on the SFTP server.");
            throw new InvalidOperationException("An error occurred while checking if the file exists on the SFTP server.", ex);
        }
        finally
        {
            if (client.IsConnected)
            {
                client.Disconnect();
            }
            client.Dispose();
        }
    }

    public async Task UploadAsync(SFTPUploadWorkItem workItem, CancellationToken cancellationToken)
    {
        _jobStore.MarkRunning(workItem.JobId);
        SftpClient client = CreateSftpClient(workItem.Connection);

        try
        {
            await client.ConnectAsync(cancellationToken);

            for (int fileIndex = 0; fileIndex < workItem.Files.Count; fileIndex++)
            {
                SFTPUploadFile file = workItem.Files[fileIndex];
                await using FileStream input = File.OpenRead(file.LocalPath);
                var progress = new Progress<UploadFileProgressReport>(report =>
                    _jobStore.ReportProgress(workItem.JobId, fileIndex, (long)report.TotalBytesUploaded));

                await client.UploadFileAsync(
                    input: input,
                    path: file.RemoteFilePath,
                    canOverride: true,
                    uploadProgress: progress,
                    cancellationToken: cancellationToken);

                _jobStore.MarkCompletedFile(workItem.JobId, fileIndex);
            }

            _jobStore.MarkCompleted(workItem.JobId);
        }
        catch (Exception ex)
        {
            _jobStore.MarkFailed(workItem.JobId, ex);
            _logger.LogError(ex, "SFTP upload job {JobId} failed", workItem.JobId);
            throw;
        }
        finally
        {
            if (client.IsConnected)
            {
                client.Disconnect();
            }

            client.Dispose();
            foreach (SFTPUploadFile file in workItem.Files)
            {
                File.Delete(file.LocalPath);
            }
        }
    }

    public async Task<SFTPActionListResult> ListFilesAsync(SFTPConnection connection, string remoteDirectoryPath, CancellationToken cancellationToken)
    {
        SftpClient client = CreateSftpClient(connection);

        try
        {
            await client.ConnectAsync(cancellationToken);
            var files = client.ListDirectoryAsync(remoteDirectoryPath, cancellationToken);
            return new SFTPActionListResult(await files.ToListAsync(cancellationToken: cancellationToken), StatusCode.Ok);
        }
        catch (SftpException ex)
        {
            switch (ex.StatusCode)
            {
                case StatusCode.NoSuchFile:
                    return new SFTPActionListResult([], StatusCode.NoSuchFile, "The specified directory does not exist on the SFTP server.");
                case StatusCode.PermissionDenied:
                    return new SFTPActionListResult([], StatusCode.PermissionDenied, "Permission denied when listing files in the specified directory on the SFTP server.");
                default:
                    _logger.LogError(ex, "An unexpected SFTP error occurred while listing files on the SFTP server.");
                    return new SFTPActionListResult([], StatusCode.Failure, $"An unexpected SFTP error occurred: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while listing files on the SFTP server.");
            throw new InvalidOperationException("An error occurred while listing files on the SFTP server.", ex);
        }
        finally
        {
            if (client.IsConnected)
            {
                client.Disconnect();
            }
            client.Dispose();
        }
    }

    public async Task<SFTPActionRenameResult> RenameFileAsync(SFTPConnection connection, string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken)
    {
        SftpClient client = CreateSftpClient(connection);

        try
        {
            await client.ConnectAsync(cancellationToken);
            await client.RenameFileAsync(sourceFilePath, destinationFilePath, cancellationToken);
            return new SFTPActionRenameResult(StatusCode.Ok);
        }
        catch (SftpException ex)
        {
            switch (ex.StatusCode)
            {
                case StatusCode.NoSuchFile:
                    return new SFTPActionRenameResult(StatusCode.NoSuchFile, "Source file or destination directory does not exist.");
                case StatusCode.PermissionDenied:
                    return new SFTPActionRenameResult(StatusCode.PermissionDenied, "Permission denied.");
                default:
                    _logger.LogError(ex, "An unexpected SFTP error occurred while renaming or moving the file on the SFTP server.");
                    return new SFTPActionRenameResult(StatusCode.Failure, $"An unexpected SFTP error occurred: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while renaming or moving the file on the SFTP server.");
            throw new InvalidOperationException("An error occurred while renaming or moving the file on the SFTP server.", ex);
        }
        finally
        {
            if (client.IsConnected)
            {
                client.Disconnect();
            }
            client.Dispose();
        }
    }

    public async Task<List<SFTPActionCreateDirectoryResult>> CreateDirectoryAsync(SFTPConnection connection, string remoteDirectoryPath, CancellationToken cancellationToken)
    {
        SftpClient client = CreateSftpClient(connection);

        string[] directoriesToCreate = remoteDirectoryPath.Split(['/'], StringSplitOptions.RemoveEmptyEntries);
        string currentPath = "";
        List<SFTPActionCreateDirectoryResult> createDirectoryResults = [];

        try
        {
            await client.ConnectAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while connecting to the SFTP server.");
            throw new InvalidOperationException("An error occurred while connecting to the SFTP server.", ex);
        }

        foreach (string directory in directoriesToCreate)
        {
            currentPath = Path.Combine(currentPath, directory).Replace("\\", "/");
            SFTPActionExistsResult existsResult = await FileExistsAsync(connection, currentPath, cancellationToken);
            if (existsResult.StatusCode == StatusCode.NoSuchFile)
            {
                try
                {
                    await client.CreateDirectoryAsync(currentPath, cancellationToken);
                    createDirectoryResults.Add(new SFTPActionCreateDirectoryResult(StatusCode.Ok, currentPath));
                }
                catch (SftpException ex)
                {
                    switch (ex.StatusCode)
                    {
                        case StatusCode.PermissionDenied:
                            createDirectoryResults.Add(new SFTPActionCreateDirectoryResult(StatusCode.PermissionDenied, currentPath, $"Permission denied when creating directory: {currentPath}"));
                            break;
                        default:
                            _logger.LogError(ex, "An unexpected SFTP error occurred while creating directory on the SFTP server.");
                            createDirectoryResults.Add(new SFTPActionCreateDirectoryResult(StatusCode.Failure, currentPath, $"An unexpected SFTP error occurred: {ex.Message}"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    if (client.IsConnected)
                    {
                        client.Disconnect();
                    }
                    client.Dispose();
                    _logger.LogError(ex, "An error occurred while creating directory on the SFTP server.");
                    throw new InvalidOperationException("An error occurred while creating directory on the SFTP server.", ex);
                }
            }
            else
            {
                createDirectoryResults.Add(new SFTPActionCreateDirectoryResult(StatusCode.Ok, currentPath));
            }
        }

        if (client.IsConnected)
        {
            client.Disconnect();
        }
        client.Dispose();

        return createDirectoryResults;
    }

    private SftpClient CreateSftpClient(SFTPConnection connection)
    {
        ArgumentException.ThrowIfNullOrEmpty(connection.PrivateKey);
        byte[] decryptedKey = _cryptoService.Decrypt(Convert.FromBase64String(connection.PrivateKey));
        string pem = _cryptoService.ConvertPrivateKeyToPem(decryptedKey);
        using var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(pem));
        PrivateKeyFile privateKey = new(keyStream);

        return new SftpClient(connection.Host, connection.Port ?? 22, "fileodon", privateKey);
    }
}
