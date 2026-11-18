using Infrastructure.Models;
using Infrastructure.Models.DTO.SFTPAction;
using Infrastructure.Worker;

namespace Domain.Interface;

public interface ISFTPActionService
{
    public Task<SFTPActionExistsResult> FileExistsAsync(SFTPConnection connection, string remoteFilePath, CancellationToken cancellationToken);
    public Task<Guid> EnqueueUploadAsync(SFTPConnection connection, IReadOnlyList<SFTPUploadInput> files, CancellationToken cancellationToken);
    public Task<Guid> CreateDistributionJobAsync(
        int organizationId,
        string userId,
        IReadOnlyList<SFTPDistributionLocation> locations,
        IReadOnlyList<SFTPUploadInput> files,
        CancellationToken cancellationToken);
    public Task<SFTPDistributionJobSnapshot?> GetDistributionJobAsync(
        Guid jobId,
        int organizationId,
        string userId,
        CancellationToken cancellationToken);
    public Task<SFTPActionListResult> ListFilesAsync(SFTPConnection connection, string remoteDirectoryPath, CancellationToken cancellationToken);
    public SFTPUploadJobSnapshot? GetUploadStatus(Guid jobId, SFTPConnection connection);
    public Task<SFTPActionRenameResult> RenameFileAsync(SFTPConnection connection, string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken);
    public Task<SFTPActionDownloadResult> DownloadFileAsync(SFTPConnection connection, string remoteFilePath, CancellationToken cancellationToken);
    public Task<List<SFTPActionCreateDirectoryResult>> CreateDirectoryAsync(SFTPConnection connection, string remoteDirectoryPath, CancellationToken cancellationToken);
    public Task<SFTPActionDeleteResult> DeleteAsync(SFTPConnection connection, string remoteFilePath, CancellationToken cancellationToken);
}
