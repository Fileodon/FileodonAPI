using Infrastructure.Models;
using Infrastructure.Models.DTO.SFTPAction;
using Infrastructure.Models.Enums;

namespace Infrastructure.Interface.Repository;

public interface ISFTPDistributionJobRepository
{
    Task CreateAndMarkReadyAsync(SFTPDistributionJob job, CancellationToken cancellationToken);
    Task<SFTPDistributionJobSnapshot?> GetAsync(
        Guid jobId,
        int organizationId,
        string userId,
        CancellationToken cancellationToken);
    Task<SFTPDistributionJobWorkItem?> ClaimNextReadyAsync(CancellationToken cancellationToken);
    Task UpdateProgressAsync(
        Guid jobId,
        SFTPDistributionJobStatus status,
        IReadOnlyList<SFTPDistributionDestinationProgress> progress,
        string? error,
        CancellationToken cancellationToken);
}
