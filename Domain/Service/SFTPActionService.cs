using System.Text;
using Domain.Interface;
using Domain.Worker;
using Infrastructure.Interface;
using Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Domain.Service;

public class SFTPActionService : ISFTPActionService
{
    private readonly ILogger<SFTPActionService> _logger;
    private readonly ICryptoService _cryptoService;
    private readonly ISFTPUploadSender _uploadSender;

    public SFTPActionService(
        ILogger<SFTPActionService> logger,
        ICryptoService cryptoService,
        ISFTPUploadSender uploadSender)
    {
        _logger = logger;
        _cryptoService = cryptoService;
        _uploadSender = uploadSender;
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

    public async Task<Guid> EnqueueUploadAsync(
        SFTPConnection connection,
        IReadOnlyList<SFTPUploadInput> files,
        CancellationToken cancellationToken)
    {
        if (files.Count == 0)
        {
            throw new ArgumentException("At least one file is required.", nameof(files));
        }

        string directory = Path.Combine(Path.GetTempPath(), "fileodon-sftp");
        Directory.CreateDirectory(directory);
        List<SFTPUploadFile> stagedFiles = [];

        try
        {
            foreach (SFTPUploadInput file in files)
            {
                string localPath = Path.Combine(directory, $"{Guid.NewGuid():N}.upload");
                await using (FileStream output = File.Create(localPath))
                {
                    await file.Content.CopyToAsync(output, cancellationToken);
                }

                stagedFiles.Add(new SFTPUploadFile(localPath, file.RemoteFilePath, file.Length));
            }

            return _uploadSender.Enqueue(new SFTPUploadJobRequest(connection, stagedFiles));
        }
        catch (Exception ex)
        {
            foreach (SFTPUploadFile file in stagedFiles)
            {
                File.Delete(file.LocalPath);
            }

            _logger.LogError(ex, "An error occurred while queueing SFTP upload files.");
            throw;
        }
    }

    public SFTPUploadJobSnapshot? GetUploadStatus(Guid jobId, SFTPConnection connection) =>
        _uploadSender.GetStatus(jobId, connection);

    private SftpClient CreateSftpClient(SFTPConnection connection)
    {
        ArgumentException.ThrowIfNullOrEmpty(connection.PrivateKey);
        string host = connection.Host;
        int port = connection.Port;
        string username = "fileodon";
        byte[] decryptedKey = _cryptoService.Decrypt(Convert.FromBase64String(connection.PrivateKey));
        string pem = _cryptoService.ConvertPrivateKeyToPem(decryptedKey);
        using var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(pem));
        PrivateKeyFile privateKey = new(keyStream);

        return new SftpClient(host, port, username, privateKey);
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
}
