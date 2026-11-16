using Infrastructure.Interface.Repository;
using Infrastructure.Interface.Repository.SFTP;
using Infrastructure.Models;
using Infrastructure.Models.DTO.SFTPAction;
using Infrastructure.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Worker;

public sealed class SFTPDistributionWorker(
    IServiceScopeFactory scopeFactory,
    ISFTPRepository sftpRepository,
    ILogger<SFTPDistributionWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            SFTPDistributionJobWorkItem? workItem;
            await using (AsyncServiceScope scope = scopeFactory.CreateAsyncScope())
            {
                ISFTPDistributionJobRepository repository =
                    scope.ServiceProvider.GetRequiredService<ISFTPDistributionJobRepository>();
                workItem = await repository.ClaimNextReadyAsync(stoppingToken);
            }

            if (workItem is null)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                continue;
            }

            try
            {
                await ProcessAsync(workItem, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing SFTP distribution job {JobId}", workItem.JobId);
                await UpdateJobAsync(
                    workItem.JobId,
                    SFTPDistributionJobStatus.Failed,
                    workItem.Progress,
                    ex.Message,
                    stoppingToken);
            }
        }
    }

    private async Task ProcessAsync(
        SFTPDistributionJobWorkItem workItem,
        CancellationToken cancellationToken)
    {
        List<SFTPDistributionDestination> destinations = [.. workItem.Destinations.Where(destination => destination.Depth == 0)];
        Dictionary<int, SFTPConnection> connections;
        await using (AsyncServiceScope scope = scopeFactory.CreateAsyncScope())
        {
            ISFTPConnectionRepository repository =
                scope.ServiceProvider.GetRequiredService<ISFTPConnectionRepository>();
            IEnumerable<SFTPConnection> resolved = await repository.GetOrganizationSFTPConnectionsAsync(
                workItem.OrganizationId,
                workItem.UserId);
            connections = resolved
                .Where(connection => destinations.Any(destination => destination.SftpConnectionId == connection.Id))
                .ToDictionary(connection => connection.Id);
        }

        if (connections.Count != destinations.Count)
        {
            throw new InvalidOperationException("One or more initial SFTP destinations could not be resolved.");
        }

        object progressLock = new();
        Dictionary<int, long[]> uploadedByFile = destinations.ToDictionary(
            destination => destination.SftpConnectionId,
            _ => new long[workItem.Files.Count]);
        using CancellationTokenSource progressCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task progressTask = PersistProgressLoopAsync(
            workItem.JobId,
            workItem.Progress,
            progressLock,
            progressCancellation.Token);

        try
        {
            foreach (SFTPDistributionDestination destination in destinations)
            {
                lock (progressLock)
                {
                    UpdateDestination(
                        workItem.Progress,
                        destination.SftpConnectionId,
                        SFTPDistributionDestinationStatus.Processing,
                        uploadedByFile[destination.SftpConnectionId],
                        workItem.Files);
                }

                try
                {
                    await sftpRepository.UploadDistributionAsync(
                        connections[destination.SftpConnectionId],
                        destination,
                        workItem.Files,
                        (fileIndex, uploadedBytes) =>
                        {
                            lock (progressLock)
                            {
                                uploadedByFile[destination.SftpConnectionId][fileIndex] = Math.Max(
                                    uploadedByFile[destination.SftpConnectionId][fileIndex],
                                    uploadedBytes);
                                UpdateDestination(
                                    workItem.Progress,
                                    destination.SftpConnectionId,
                                    SFTPDistributionDestinationStatus.Processing,
                                    uploadedByFile[destination.SftpConnectionId],
                                    workItem.Files);
                            }
                        },
                        cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    lock (progressLock)
                    {
                        UpdateDestination(
                            workItem.Progress,
                            destination.SftpConnectionId,
                            SFTPDistributionDestinationStatus.Failed,
                            uploadedByFile[destination.SftpConnectionId],
                            workItem.Files,
                            ex.Message);
                    }

                    throw;
                }

                lock (progressLock)
                {
                    UpdateDestination(
                        workItem.Progress,
                        destination.SftpConnectionId,
                        SFTPDistributionDestinationStatus.Completed,
                        uploadedByFile[destination.SftpConnectionId],
                        workItem.Files);
                }
            }

            progressCancellation.Cancel();
            await progressTask;
            await UpdateJobAsync(
                workItem.JobId,
                SFTPDistributionJobStatus.Completed,
                workItem.Progress,
                null,
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            progressCancellation.Cancel();
            await progressTask;
            await UpdateJobAsync(
                workItem.JobId,
                SFTPDistributionJobStatus.Failed,
                workItem.Progress,
                ex.Message,
                cancellationToken);
            throw;
        }
        finally
        {
            progressCancellation.Cancel();
            await progressTask;
            foreach (SFTPDistributionFile file in workItem.Files)
            {
                try
                {
                    File.Delete(file.LocalPath);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Failed to delete staged distribution file {LocalPath} for job {JobId}",
                        file.LocalPath,
                        workItem.JobId);
                }
            }
        }
    }

    private async Task PersistProgressLoopAsync(
        Guid jobId,
        IReadOnlyList<SFTPDistributionDestinationProgress> progress,
        object progressLock,
        CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(2));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await PersistProgressAsync(jobId, progress, progressLock, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private async Task PersistProgressAsync(
        Guid jobId,
        IReadOnlyList<SFTPDistributionDestinationProgress> progress,
        object progressLock,
        CancellationToken cancellationToken)
    {
        List<SFTPDistributionDestinationProgress> snapshot;
        lock (progressLock)
        {
            snapshot = [.. progress];
        }

        await UpdateJobAsync(
            jobId,
            SFTPDistributionJobStatus.Processing,
            snapshot,
            null,
            cancellationToken);
    }

    private async Task UpdateJobAsync(
        Guid jobId,
        SFTPDistributionJobStatus status,
        IReadOnlyList<SFTPDistributionDestinationProgress> progress,
        string? error,
        CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        ISFTPDistributionJobRepository repository =
            scope.ServiceProvider.GetRequiredService<ISFTPDistributionJobRepository>();
        await repository.UpdateProgressAsync(jobId, status, progress, error, cancellationToken);
    }

    private static void UpdateDestination(
        List<SFTPDistributionDestinationProgress> progress,
        int connectionId,
        SFTPDistributionDestinationStatus status,
        IReadOnlyList<long> uploadedByFile,
        IReadOnlyList<SFTPDistributionFile> files,
        string? error = null)
    {
        int index = progress
            .Select((item, index) => (item, index))
            .First(item => item.item.SftpConnectionId == connectionId)
            .index;
        int completedFiles = uploadedByFile
            .Select((uploaded, fileIndex) => uploaded >= files[fileIndex].Length)
            .Count(completed => completed);
        SFTPDistributionDestinationProgress updated = progress[index] with
        {
            Status = status,
            CompletedFiles = completedFiles,
            UploadedBytes = uploadedByFile.Sum(),
            Error = error
        };
        progress[index] = updated;
    }
}
