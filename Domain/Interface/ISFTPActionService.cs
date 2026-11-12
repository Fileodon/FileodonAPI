using Domain.DTO;
using Domain.Worker;
using Infrastructure.Models;
using Renci.SshNet.Sftp;

namespace Domain.Interface;

public interface ISFTPActionService
{
    public Task<bool> FileExistsAsync(SFTPConnection connection, string remoteFilePath);
    public Task<Guid> EnqueueUploadAsync(SFTPConnection connection, IReadOnlyList<SFTPUploadInput> files, CancellationToken cancellationToken);
    public Task<IEnumerable<ISftpFile>> ListFilesAsync(SFTPConnection connection, string remoteDirectoryPath, CancellationToken cancellationToken);
    public SFTPUploadJobSnapshot? GetUploadStatus(Guid jobId, SFTPConnection connection);
}
