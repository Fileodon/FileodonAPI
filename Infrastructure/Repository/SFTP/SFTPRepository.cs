using System.Text;
using Infrastructure.Interface;
using Infrastructure.Interface.Repository.SFTP;
using Infrastructure.Models;
using Infrastructure.Worker;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
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

    public async Task<bool> FileExistsAsync(SFTPConnection connection, string remoteFilePath)
    {
        SftpClient client = CreateSftpClient(connection);

        try
        {
            await client.ConnectAsync(CancellationToken.None);
            bool fileExists = await client.ExistsAsync(remoteFilePath);
            return fileExists;
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

    public async Task<IEnumerable<ISftpFile>> ListFilesAsync(SFTPConnection connection, string remoteDirectoryPath, CancellationToken cancellationToken)
    {
        SftpClient client = CreateSftpClient(connection);

        try
        {
            await client.ConnectAsync(cancellationToken);
            var files = client.ListDirectoryAsync(remoteDirectoryPath, cancellationToken);
            return await files.ToListAsync(cancellationToken: cancellationToken);
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

    private SftpClient CreateSftpClient(Infrastructure.Models.SFTPConnection connection)
    {
        ArgumentException.ThrowIfNullOrEmpty(connection.PrivateKey);
        byte[] decryptedKey = _cryptoService.Decrypt(Convert.FromBase64String(connection.PrivateKey));
        string pem = _cryptoService.ConvertPrivateKeyToPem(decryptedKey);
        using var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(pem));
        PrivateKeyFile privateKey = new(keyStream);

        return new SftpClient(connection.Host, connection.Port, "fileodon", privateKey);
    }
}
