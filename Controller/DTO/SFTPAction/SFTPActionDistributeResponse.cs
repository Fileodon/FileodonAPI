using Infrastructure.Models.DTO.SFTPAction;
using Infrastructure.Models.Enums;

namespace Controller.DTO.SFTPAction;

public sealed record SFTPActionDistributeResponse(
    Guid JobId,
    SFTPDistributionJobStatus Status,
    int TotalFiles,
    long TotalBytes,
    IReadOnlyList<SFTPDistributionDestination> Destinations,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadyAt);

public static class SFTPActionDistributeResponseExtensions
{
    public static SFTPActionDistributeResponse ToResponse(this SFTPDistributionJobSnapshot snapshot) =>
        new(
            snapshot.JobId,
            snapshot.Status,
            snapshot.TotalFiles,
            snapshot.TotalBytes,
            snapshot.Destinations,
            snapshot.CreatedAt,
            snapshot.ReadyAt);
}
