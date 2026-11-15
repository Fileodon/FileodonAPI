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
            await client.ConnectAsync(cancellationToken);
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
                try
                {
                    File.Delete(file.LocalPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete local temporary file {LocalPath} after SFTP upload job {JobId}", file.LocalPath, workItem.JobId);
                }
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

    public async Task<SFTPActionDownloadResult> DownloadFileAsync(SFTPConnection connection, string remoteFilePath, CancellationToken cancellationToken)
    {
        SftpClient client = CreateSftpClient(connection);

        try
        {
            await client.ConnectAsync(cancellationToken);
            SftpFileStream fileStream = await client.OpenAsync(remoteFilePath, FileMode.Open, FileAccess.Read, cancellationToken);
            ISftpFile fileRef = await client.GetAsync(remoteFilePath, cancellationToken);
            return new SFTPActionDownloadResult(StatusCode.Ok, Stream: new SftpClientOwnedStream(fileStream, client), FileName: fileRef.Name);
        }
        catch (SftpException ex)
        {
            if (client.IsConnected)
            {
                client.Disconnect();
            }
            client.Dispose();
            switch (ex.StatusCode)
            {
                case StatusCode.NoSuchFile:
                    return new SFTPActionDownloadResult(StatusCode.NoSuchFile, "The specified file does not exist on the SFTP server.");
                case StatusCode.PermissionDenied:
                    return new SFTPActionDownloadResult(StatusCode.PermissionDenied, "Permission denied when downloading the file from the SFTP server.");
                default:
                    _logger.LogError(ex, "An unexpected SFTP error occurred while downloading the file from the SFTP server.");
                    return new SFTPActionDownloadResult(StatusCode.Failure, $"An unexpected SFTP error occurred: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            if (client.IsConnected)
            {
                client.Disconnect();
            }
            client.Dispose();
            _logger.LogError(ex, "An error occurred while downloading the file from the SFTP server.");
            throw new InvalidOperationException("An error occurred while downloading the file from the SFTP server.", ex);
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
            foreach (string directory in directoriesToCreate)
            {
                currentPath = Path.Combine(currentPath, directory).Replace("\\", "/");
                bool pathExists = await client.ExistsAsync(currentPath, cancellationToken);
                if (pathExists is false)
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
                                createDirectoryResults.Add(new SFTPActionCreateDirectoryResult(StatusCode.PermissionDenied, currentPath, "Permission denied when creating the directory on the SFTP server."));
                                break;
                            default:
                                _logger.LogError(ex, "An unexpected SFTP error occurred while creating the directory on the SFTP server.");
                                createDirectoryResults.Add(new SFTPActionCreateDirectoryResult(StatusCode.Failure, currentPath, $"An unexpected SFTP error occurred: {ex.Message}"));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while creating the directory on the SFTP server.");
                        throw new InvalidOperationException("An error occurred while creating the directory on the SFTP server.", ex);
                    }
                }
                else
                {
                    try
                    {
                        ISftpFile fileRef = await client.GetAsync(currentPath, cancellationToken);
                        if (fileRef.IsDirectory is false)
                        {
                            createDirectoryResults.Add(new SFTPActionCreateDirectoryResult(StatusCode.Failure, currentPath, "A file with the same name already exists on the SFTP server."));
                        }
                        else
                        {
                            createDirectoryResults.Add(new SFTPActionCreateDirectoryResult(StatusCode.Ok, currentPath));
                        }
                    }
                    catch (SftpException ex)
                    {
                        switch (ex.StatusCode)
                        {
                            case StatusCode.NoSuchFile:
                                createDirectoryResults.Add(new SFTPActionCreateDirectoryResult(StatusCode.NoSuchFile, currentPath, "The specified directory does not exist on the SFTP server."));
                                break;
                            case StatusCode.PermissionDenied:
                                createDirectoryResults.Add(new SFTPActionCreateDirectoryResult(StatusCode.PermissionDenied, currentPath, "Permission denied when checking the directory on the SFTP server."));
                                break;
                            default:
                                _logger.LogError(ex, "An unexpected SFTP error occurred while checking the directory on the SFTP server.");
                                createDirectoryResults.Add(new SFTPActionCreateDirectoryResult(StatusCode.Failure, currentPath, $"An unexpected SFTP error occurred: {ex.Message}"));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while checking the directory on the SFTP server.");
                        throw new InvalidOperationException("An error occurred while checking the directory on the SFTP server.", ex);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while connecting to the SFTP server.");
            throw new InvalidOperationException("An error occurred while connecting to the SFTP server.", ex);
        }
        finally
        {
            if (client.IsConnected)
            {
                client.Disconnect();
            }
            client.Dispose();
        }

        return createDirectoryResults;
    }

    public async Task<List<SFTPActionDeleteResult>> DeleteAsync(SFTPConnection connection, string remoteFilePath, CancellationToken cancellationToken)
    {
        SftpClient client = CreateSftpClient(connection);

        List<SFTPActionDeleteResult> deleteResults = [];

        try
        {
            await client.ConnectAsync(cancellationToken);
            bool pathExists = await client.ExistsAsync(remoteFilePath, cancellationToken);

            if (pathExists is false)
            {
                deleteResults.Add(new SFTPActionDeleteResult(StatusCode.NoSuchFile, remoteFilePath, "The specified file or directory does not exist on the SFTP server."));
                return deleteResults;
            }

            ISftpFile fileRef = await client.GetAsync(remoteFilePath, cancellationToken);
            if (fileRef.IsDirectory)
            {
                deleteResults = await DeleteFolderAsync(client, remoteFilePath, deleteResults, cancellationToken);
            }
            else
            {
                deleteResults = await DeleteFileAsync(client, remoteFilePath, deleteResults, cancellationToken);
            }

            return deleteResults;
        }
        catch (SftpException ex)
        {
            switch (ex.StatusCode)
            {
                case StatusCode.NoSuchFile:
                    deleteResults.Add(new SFTPActionDeleteResult(StatusCode.NoSuchFile, remoteFilePath, "The specified file or directory does not exist on the SFTP server."));
                    return deleteResults;
                case StatusCode.PermissionDenied:
                    deleteResults.Add(new SFTPActionDeleteResult(StatusCode.PermissionDenied, remoteFilePath, "Permission denied when deleting the file or directory on the SFTP server."));
                    return deleteResults;
                default:
                    _logger.LogError(ex, "An unexpected SFTP error occurred while deleting the file or directory on the SFTP server.");
                    deleteResults.Add(new SFTPActionDeleteResult(StatusCode.Failure, remoteFilePath, $"An unexpected SFTP error occurred: {ex.Message}"));
                    return deleteResults;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the file or directory on the SFTP server.");
            throw new InvalidOperationException("An error occurred while deleting the file or directory on the SFTP server.", ex);
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

    private async Task<List<SFTPActionDeleteResult>> DeleteFolderAsync(SftpClient client, string currentPath, List<SFTPActionDeleteResult> deleteResults, CancellationToken cancellationToken)
    {
        IAsyncEnumerable<ISftpFile> files;
        try
        {
            files = client.ListDirectoryAsync(currentPath, cancellationToken);
        }
        catch (SftpException ex)
        {
            switch (ex.StatusCode)
            {
                case StatusCode.NoSuchFile:
                    deleteResults.Add(new SFTPActionDeleteResult(StatusCode.NoSuchFile, currentPath, "The specified directory does not exist on the SFTP server."));
                    return deleteResults;
                case StatusCode.PermissionDenied:
                    deleteResults.Add(new SFTPActionDeleteResult(StatusCode.PermissionDenied, currentPath, "Permission denied when listing files in the specified directory on the SFTP server."));
                    return deleteResults;
                default:
                    _logger.LogError(ex, "An unexpected SFTP error occurred while listing files in the specified directory on the SFTP server.");
                    deleteResults.Add(new SFTPActionDeleteResult(StatusCode.Failure, currentPath, $"An unexpected SFTP error occurred: {ex.Message}"));
                    return deleteResults;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while listing files in the specified directory on the SFTP server.");
            throw new InvalidOperationException("An error occurred while listing files in the specified directory on the SFTP server.", ex);
        }

        await foreach (ISftpFile file in files)
        {
            if (file.Name == "." || file.Name == "..")
            {
                continue;
            }

            if (file.IsDirectory)
            {
                deleteResults = await DeleteFolderAsync(client, file.FullName, deleteResults, cancellationToken);
            }
            else
            {
                deleteResults = await DeleteFileAsync(client, file.FullName, deleteResults, cancellationToken);
            }
        }

        try
        {
            await client.DeleteDirectoryAsync(currentPath, cancellationToken);
            deleteResults.Add(new SFTPActionDeleteResult(StatusCode.Ok, currentPath));
            return deleteResults;
        }
        catch (SftpException ex)
        {
            switch (ex.StatusCode)
            {
                case StatusCode.NoSuchFile:
                    deleteResults.Add(new SFTPActionDeleteResult(StatusCode.NoSuchFile, currentPath, "The specified directory does not exist on the SFTP server."));
                    break;
                case StatusCode.PermissionDenied:
                    deleteResults.Add(new SFTPActionDeleteResult(StatusCode.PermissionDenied, currentPath, "Permission denied when deleting the directory on the SFTP server."));
                    break;
                default:
                    _logger.LogError(ex, "An unexpected SFTP error occurred while deleting the directory on the SFTP server.");
                    deleteResults.Add(new SFTPActionDeleteResult(StatusCode.Failure, currentPath, $"An unexpected SFTP error occurred: {ex.Message}"));
                    break;
            }
            return deleteResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the directory on the SFTP server.");
            throw new InvalidOperationException("An error occurred while deleting the directory on the SFTP server.", ex);
        }
    }

    private async Task<List<SFTPActionDeleteResult>> DeleteFileAsync(SftpClient client, string filePath, List<SFTPActionDeleteResult> deleteResults, CancellationToken cancellationToken)
    {
        try
        {
            await client.DeleteFileAsync(filePath, cancellationToken);
            deleteResults.Add(new SFTPActionDeleteResult(StatusCode.Ok, filePath));
            return deleteResults;
        }
        catch (SftpException ex)
        {
            switch (ex.StatusCode)
            {
                case StatusCode.NoSuchFile:
                    deleteResults.Add(new SFTPActionDeleteResult(StatusCode.NoSuchFile, filePath, "The specified file does not exist on the SFTP server."));
                    break;
                case StatusCode.PermissionDenied:
                    deleteResults.Add(new SFTPActionDeleteResult(StatusCode.PermissionDenied, filePath, "Permission denied when deleting the file on the SFTP server."));
                    break;
                default:
                    _logger.LogError(ex, "An unexpected SFTP error occurred while deleting the file on the SFTP server.");
                    deleteResults.Add(new SFTPActionDeleteResult(StatusCode.Failure, filePath, $"An unexpected SFTP error occurred: {ex.Message}"));
                    break;
            }
            return deleteResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the file on the SFTP server.");
            throw new InvalidOperationException("An error occurred while deleting the file on the SFTP server.", ex);
        }
    }

    private SftpClient CreateSftpClient(SFTPConnection connection)
    {
        ArgumentException.ThrowIfNullOrEmpty(connection.PrivateKey);
        byte[] decryptedKey = _cryptoService.Decrypt(Convert.FromBase64String(connection.PrivateKey));
        string pem = _cryptoService.ConvertPrivateKeyToPem(decryptedKey);
        using var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(pem));
        PrivateKeyFile privateKey = new(keyStream);

        return new SftpClient(connection.Host, connection.Port, "fileodon", privateKey);
    }
}
