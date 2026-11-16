using Infrastructure.Models;
using Infrastructure.Models.DTO.SFTPAction;

namespace Infrastructure.Interface.Repository;

public interface ISFTPDistributionJobRepository
{
    Task CreateAndMarkReadyAsync(SFTPDistributionJob job, CancellationToken cancellationToken);
    Task<SFTPDistributionJobSnapshot?> GetAsync(
        Guid jobId,
        int organizationId,
        string userId,
        CancellationToken cancellationToken);
}
