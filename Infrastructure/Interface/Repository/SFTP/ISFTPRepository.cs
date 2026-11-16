using Infrastructure.Models;
using Infrastructure.Models.DTO.SFTPAction;
using Infrastructure.Worker;

namespace Infrastructure.Interface.Repository.SFTP;

public interface ISFTPRepository
{
    public Task<SFTPActionExistsResult> FileExistsAsync(SFTPConnection connection, string remoteFilePath, CancellationToken cancellationToken);
    Task UploadAsync(SFTPUploadWorkItem workItem, CancellationToken cancellationToken);
    Task UploadDistributionAsync(
        SFTPConnection connection,
        SFTPDistributionDestination destination,
        IReadOnlyList<SFTPDistributionFile> files,
        string pamToken,
        Action<int, long> progress,
        CancellationToken cancellationToken);
    public Task<SFTPActionListResult> ListFilesAsync(SFTPConnection connection, string remoteDirectoryPath, CancellationToken cancellationToken);
    public Task<SFTPActionRenameResult> RenameFileAsync(SFTPConnection connection, string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken);
    public Task<SFTPActionDownloadResult> DownloadFileAsync(SFTPConnection connection, string remoteFilePath, CancellationToken cancellationToken);
    public Task<List<SFTPActionCreateDirectoryResult>> CreateDirectoryAsync(SFTPConnection connection, string remoteDirectoryPath, CancellationToken cancellationToken);
    public Task<SFTPActionDeleteResult> DeleteAsync(SFTPConnection connection, string remoteFilePath, CancellationToken cancellationToken);
}
