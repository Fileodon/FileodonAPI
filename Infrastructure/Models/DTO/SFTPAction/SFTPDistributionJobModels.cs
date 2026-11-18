using Infrastructure.Models.Enums;

namespace Infrastructure.Models.DTO.SFTPAction;

public sealed record SFTPDistributionLocation(int SftpConnectionId, string RemotePath);

public sealed record SFTPDistributionDestination(
    int SftpConnectionId,
    int? ParentConnectionId,
    string RemotePath,
    int Depth);

public sealed record SFTPDistributionFile(string LocalPath, string RemotePath, long Length);

public sealed record SFTPDistributionDestinationProgress(
    int SftpConnectionId,
    SFTPDistributionDestinationStatus Status,
    int CompletedFiles,
    long UploadedBytes,
    string? Error);

public sealed record SFTPDistributionJobSnapshot(
    Guid JobId,
    SFTPDistributionJobStatus Status,
    int TotalFiles,
    long TotalBytes,
    IReadOnlyList<SFTPDistributionDestination> Destinations,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadyAt,
    int CompletedFiles,
    long UploadedBytes,
    double ProgressPercentage,
    IReadOnlyList<SFTPDistributionDestinationProgress> DestinationProgress,
    string? Error);

public sealed record SFTPDistributionJobWorkItem(
    Guid JobId,
    int OrganizationId,
    string UserId,
    IReadOnlyList<SFTPDistributionFile> Files,
    IReadOnlyList<SFTPDistributionDestination> Destinations,
    List<SFTPDistributionDestinationProgress> Progress);
