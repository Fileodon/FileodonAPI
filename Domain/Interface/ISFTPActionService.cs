using Domain.DTO;
using Domain.Worker;
using Infrastructure.Models;

namespace Domain.Interface;

public interface ISFTPActionService
{
    public Task<bool> FileExistsAsync(SFTPConnection connection, string remoteFilePath);
    public Task<Guid> EnqueueUploadAsync(SFTPConnection connection, IReadOnlyList<SFTPUploadInput> files, CancellationToken cancellationToken);
    public SFTPUploadJobSnapshot? GetUploadStatus(Guid jobId, SFTPConnection connection);
}
