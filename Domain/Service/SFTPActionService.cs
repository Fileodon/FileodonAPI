using System.Text;
using Domain.Interface;
using Infrastructure.Interface;
using Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace Domain.Service;

public class SFTPActionService : ISFTPActionService
{
    private readonly ILogger<SFTPActionService> _logger;
    private readonly ICryptoService _cryptoService;

    public SFTPActionService(ILogger<SFTPActionService> logger, ICryptoService cryptoService)
    {
        _logger = logger;
        _cryptoService = cryptoService;
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

    public async Task<bool> UploadFileAsync(SFTPConnection connection, Stream fileStream, string remoteFilePath)
    {
        SftpClient client = CreateSftpClient(connection);

        try
        {
            await client.ConnectAsync(CancellationToken.None);
            // Always override. File clashing produces unhandleable exceptions, so we check before attempting the upload
            await client.UploadFileAsync(fileStream, remoteFilePath, canOverride: true);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while uploading the file to the SFTP server.");
            throw new InvalidOperationException("An error occurred while uploading the file to the SFTP server.", ex);
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
}
