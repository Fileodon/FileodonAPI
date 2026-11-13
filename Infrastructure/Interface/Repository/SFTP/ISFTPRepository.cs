using Infrastructure.Models;
using Infrastructure.Worker;
using Renci.SshNet.Sftp;

namespace Infrastructure.Interface.Repository.SFTP;

public interface ISFTPRepository
{
    public Task<bool> FileExistsAsync(SFTPConnection connection, string remoteFilePath);
    Task UploadAsync(SFTPUploadWorkItem workItem, CancellationToken cancellationToken);
    public Task<IEnumerable<ISftpFile>> ListFilesAsync(SFTPConnection connection, string remoteDirectoryPath, CancellationToken cancellationToken);

}
