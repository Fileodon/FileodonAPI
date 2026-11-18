using System.Text.Json;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Infrastructure.Models.DTO.SFTPAction;
using Infrastructure.Models.Enums;
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
            job.ReadyAt,
            GetCompletedFiles(job),
            GetUploadedBytes(job),
            GetProgressPercentage(job),
            JsonSerializer.Deserialize<List<SFTPDistributionDestinationProgress>>(job.ProgressJson) ?? [],
            job.Error);
    }

    public async Task<SFTPDistributionJobWorkItem?> ClaimNextReadyAsync(
        CancellationToken cancellationToken)
    {
        while (true)
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            Guid? jobId = await context.SFTPDistributionJobs
                .Where(item => item.Status == SFTPDistributionJobStatus.Ready)
                .OrderBy(item => item.CreatedAt)
                .Select(item => (Guid?)item.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (jobId is null)
            {
                return null;
            }

            int claimed = await context.SFTPDistributionJobs
                .Where(item => item.Id == jobId && item.Status == SFTPDistributionJobStatus.Ready)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        item => item.Status,
                        SFTPDistributionJobStatus.Processing),
                    cancellationToken);
            if (claimed == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                continue;
            }

            SFTPDistributionJob job = await context.SFTPDistributionJobs
                .SingleAsync(item => item.Id == jobId, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new SFTPDistributionJobWorkItem(
                job.Id,
                job.OrganizationId,
                job.UserId,
                JsonSerializer.Deserialize<List<SFTPDistributionFile>>(job.FilesJson) ?? [],
                JsonSerializer.Deserialize<List<SFTPDistributionDestination>>(job.DestinationsJson) ?? [],
                JsonSerializer.Deserialize<List<SFTPDistributionDestinationProgress>>(job.ProgressJson) ?? []);
        }
    }

    public async Task UpdateProgressAsync(
        Guid jobId,
        SFTPDistributionJobStatus status,
        IReadOnlyList<SFTPDistributionDestinationProgress> progress,
        string? error,
        CancellationToken cancellationToken)
    {
        SFTPDistributionJob? job = await context.SFTPDistributionJobs
            .SingleOrDefaultAsync(item => item.Id == jobId, cancellationToken);
        if (job is null)
        {
            throw new KeyNotFoundException($"The distribution job {jobId} was not found.");
        }

        job.Status = status;
        job.ProgressJson = JsonSerializer.Serialize(progress);
        job.Error = error;
        await context.SaveChangesAsync(cancellationToken);
    }

    private static int GetCompletedFiles(SFTPDistributionJob job) =>
        JsonSerializer.Deserialize<List<SFTPDistributionDestinationProgress>>(job.ProgressJson)?
            .Sum(item => item.CompletedFiles) ?? 0;

    private static long GetUploadedBytes(SFTPDistributionJob job) =>
        JsonSerializer.Deserialize<List<SFTPDistributionDestinationProgress>>(job.ProgressJson)?
            .Sum(item => item.UploadedBytes) ?? 0;

    private static double GetProgressPercentage(SFTPDistributionJob job)
    {
        long totalBytes = job.TotalBytes *
            (JsonSerializer.Deserialize<List<SFTPDistributionDestinationProgress>>(job.ProgressJson)?.Count ?? 0);
        long uploadedBytes = GetUploadedBytes(job);
        return totalBytes == 0 ? 100 : uploadedBytes * 100d / totalBytes;
    }
}
