using Infrastructure.Models;
using Infrastructure.Models.DTO.SFTPAction;
using Infrastructure.Worker;
using Renci.SshNet.Sftp;

namespace Domain.Interface;

public interface ISFTPActionService
{
    public Task<SFTPActionExistsResult> FileExistsAsync(SFTPConnection connection, string remoteFilePath, CancellationToken cancellationToken);
    public Task<Guid> EnqueueUploadAsync(SFTPConnection connection, IReadOnlyList<SFTPUploadInput> files, CancellationToken cancellationToken);
    public Task<SFTPActionListResult> ListFilesAsync(SFTPConnection connection, string remoteDirectoryPath, CancellationToken cancellationToken);
    public SFTPUploadJobSnapshot? GetUploadStatus(Guid jobId, SFTPConnection connection);
    public Task<SFTPActionRenameResult> RenameFileAsync(SFTPConnection connection, string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken);
}
