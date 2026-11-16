using Infrastructure.Models.Enums;

namespace Infrastructure.Models.DTO.SFTPAction;

public sealed record SFTPDistributionLocation(int SftpConnectionId, string RemotePath);

public sealed record SFTPDistributionDestination(
    int SftpConnectionId,
    int? ParentConnectionId,
    string RemotePath,
    int Depth);

public sealed record SFTPDistributionFile(string LocalPath, string RemotePath, long Length);

public sealed record SFTPDistributionJobSnapshot(
    Guid JobId,
    SFTPDistributionJobStatus Status,
    int TotalFiles,
    long TotalBytes,
    IReadOnlyList<SFTPDistributionDestination> Destinations,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadyAt);
