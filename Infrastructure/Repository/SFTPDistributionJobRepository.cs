using System.Text.Json;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Infrastructure.Models.DTO.SFTPAction;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public sealed class SFTPDistributionJobRepository(ApplicationDbContext context)
    : ISFTPDistributionJobRepository
{
    public async Task CreateAndMarkReadyAsync(
        SFTPDistributionJob job,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        context.SFTPDistributionJobs.Add(job);
        await context.SaveChangesAsync(cancellationToken);

        job.Status = Models.Enums.SFTPDistributionJobStatus.Ready;
        job.ReadyAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<SFTPDistributionJobSnapshot?> GetAsync(
        Guid jobId,
        int organizationId,
        string userId,
        CancellationToken cancellationToken)
    {
        SFTPDistributionJob? job = await context.SFTPDistributionJobs
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == jobId &&
                        item.OrganizationId == organizationId &&
                        item.UserId == userId,
                cancellationToken);
        if (job is null)
        {
            return null;
        }

        return new SFTPDistributionJobSnapshot(
            job.Id,
            job.Status,
            job.TotalFiles,
            job.TotalBytes,
            JsonSerializer.Deserialize<List<SFTPDistributionDestination>>(job.DestinationsJson) ?? [],
            job.CreatedAt,
            job.ReadyAt);
    }
}
