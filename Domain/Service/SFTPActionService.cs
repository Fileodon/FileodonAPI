using System.Text.Json;
using Domain.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Interface.Repository.SFTP;
using Infrastructure.Models;
using Infrastructure.Models.DTO.SFTPAction;
using Infrastructure.Models.Enums;
using Infrastructure.Worker;
using Microsoft.Extensions.Logging;

namespace Domain.Service;

public class SFTPActionService : ISFTPActionService
{
    private readonly ILogger<SFTPActionService> _logger;
    private readonly ISFTPRepository _sFTPRepository;
    private readonly ISFTPUploadSender _uploadSender;
    private readonly ISFTPDistributionJobRepository _distributionJobRepository;

    public SFTPActionService(
        ILogger<SFTPActionService> logger,
        ISFTPRepository sFTPRepository,
        ISFTPUploadSender uploadSender,
        ISFTPDistributionJobRepository distributionJobRepository)
    {
        _logger = logger;
        _sFTPRepository = sFTPRepository;
        _uploadSender = uploadSender;
        _distributionJobRepository = distributionJobRepository;
    }

    public async Task<Guid> CreateDistributionJobAsync(
        int organizationId,
        string userId,
        IReadOnlyList<SFTPDistributionLocation> locations,
        IReadOnlyList<SFTPUploadInput> files,
        CancellationToken cancellationToken)
    {
        if (locations.Count == 0)
            throw new ArgumentException("At least one destination is required.", nameof(locations));
        if (files.Count == 0)
            throw new ArgumentException("At least one file is required.", nameof(files));

        string directory = Path.Combine(Path.GetTempPath(), "fileodon-sftp-distribution");
        Directory.CreateDirectory(directory);
        List<SFTPDistributionFile> stagedFiles = [];
        Guid jobId = Guid.NewGuid();

        try
        {
            foreach (SFTPUploadInput file in files)
            {
                string localPath = Path.Combine(directory, $"{Guid.NewGuid():N}.upload");
                await using (FileStream output = File.Create(localPath))
                {
                    await file.Content.CopyToAsync(output, cancellationToken);
                }

                stagedFiles.Add(new SFTPDistributionFile(localPath, file.RemoteFilePath, file.Length));
            }

            List<SFTPDistributionDestination> destinations = BuildDistributionPlan(locations);
            SFTPDistributionJob job = new()
            {
                Id = jobId,
                OrganizationId = organizationId,
                UserId = userId,
                Status = SFTPDistributionJobStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow,
                TotalFiles = stagedFiles.Count,
                TotalBytes = stagedFiles.Sum(file => file.Length),
                FilesJson = JsonSerializer.Serialize(stagedFiles),
                DestinationsJson = JsonSerializer.Serialize(destinations)
            };

            await _distributionJobRepository.CreateAndMarkReadyAsync(job, cancellationToken);
            return jobId;
        }
        catch
        {
            foreach (SFTPDistributionFile file in stagedFiles)
                File.Delete(file.LocalPath);
            throw;
        }
    }

    public async Task<SFTPDistributionJobSnapshot?> GetDistributionJobAsync(
        Guid jobId,
        int organizationId,
        string userId,
        CancellationToken cancellationToken)
    {
        ISFTPDistributionJobRepository distributionJobRepository = _distributionJobRepository
            ?? throw new InvalidOperationException("The distribution job repository is required.");
        return await distributionJobRepository.GetAsync(
            jobId,
            organizationId,
            userId,
            cancellationToken);
    }

    private static List<SFTPDistributionDestination> BuildDistributionPlan(
        IReadOnlyList<SFTPDistributionLocation> locations)
    {
        if (locations.Any(location => string.IsNullOrWhiteSpace(location.RemotePath)))
            throw new ArgumentException("Every destination must have a remote path.", nameof(locations));
        if (locations.Select(location => location.SftpConnectionId).Distinct().Count() != locations.Count)
            throw new ArgumentException("Each SFTP connection may only be selected once.", nameof(locations));

        int initialCount = Math.Max(1, (locations.Count + 3) / 4);
        List<SFTPDistributionDestination> plan = [];
        int nextLocation = 0;
        List<SFTPDistributionDestination> parents = [];

        for (int index = 0; index < initialCount; index++)
        {
            SFTPDistributionLocation location = locations[nextLocation++];
            SFTPDistributionDestination destination = new(location.SftpConnectionId, null, location.RemotePath, 0);
            plan.Add(destination);
            parents.Add(destination);
        }

        int depth = 1;
        while (nextLocation < locations.Count)
        {
            int childrenPerParent = 1 << Math.Min(depth, 30);
            List<SFTPDistributionDestination> nextParents = [];
            foreach (SFTPDistributionDestination parent in parents)
            {
                for (int child = 0; child < childrenPerParent && nextLocation < locations.Count; child++)
                {
                    SFTPDistributionLocation location = locations[nextLocation++];
                    SFTPDistributionDestination destination = new(
                        location.SftpConnectionId,
                        parent.SftpConnectionId,
                        location.RemotePath,
                        depth);
                    plan.Add(destination);
                    nextParents.Add(destination);
                }
            }

            parents = nextParents;
            depth++;
        }

        return plan;
    }

    public Task<SFTPActionDownloadResult> DownloadFileAsync(SFTPConnection connection, string remoteFilePath, CancellationToken cancellationToken)
    {
        return _sFTPRepository.DownloadFileAsync(connection, remoteFilePath, cancellationToken);
    }

    public Task<List<SFTPActionCreateDirectoryResult>> CreateDirectoryAsync(SFTPConnection connection, string remoteDirectoryPath, CancellationToken cancellationToken)
    {
        return _sFTPRepository.CreateDirectoryAsync(connection, remoteDirectoryPath, cancellationToken);
    }

    public async Task<Guid> EnqueueUploadAsync(
        SFTPConnection connection,
        IReadOnlyList<SFTPUploadInput> files,
        CancellationToken cancellationToken)
    {
        if (files.Count == 0)
        {
            throw new ArgumentException("At least one file is required.", nameof(files));
        }

        string directory = Path.Combine(Path.GetTempPath(), "fileodon-sftp");
        Directory.CreateDirectory(directory);
        List<SFTPUploadFile> stagedFiles = [];

        try
        {
            foreach (SFTPUploadInput file in files)
            {
                string localPath = Path.Combine(directory, $"{Guid.NewGuid():N}.upload");
                await using (FileStream output = File.Create(localPath))
                {
                    await file.Content.CopyToAsync(output, cancellationToken);
                }

                stagedFiles.Add(new SFTPUploadFile(localPath, file.RemoteFilePath, file.Length));
            }

            return _uploadSender.Enqueue(new SFTPUploadJobRequest(connection, stagedFiles));
        }
        catch (Exception ex)
        {
            foreach (SFTPUploadFile file in stagedFiles)
            {
                File.Delete(file.LocalPath);
            }

            _logger.LogError(ex, "An error occurred while queueing SFTP upload files.");
            throw;
        }
    }

    public async Task<SFTPActionExistsResult> FileExistsAsync(SFTPConnection connection, string remoteFilePath, CancellationToken cancellationToken)
    {
        return await _sFTPRepository.FileExistsAsync(connection, remoteFilePath, cancellationToken);
    }

    public SFTPUploadJobSnapshot? GetUploadStatus(Guid jobId, SFTPConnection connection) =>
        _uploadSender.GetStatus(jobId, connection);

    public async Task<SFTPActionListResult> ListFilesAsync(SFTPConnection connection, string remoteDirectoryPath, CancellationToken cancellationToken)
    {
        return await _sFTPRepository.ListFilesAsync(connection, remoteDirectoryPath, cancellationToken);
    }

    public async Task<SFTPActionRenameResult> RenameFileAsync(SFTPConnection connection, string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken)
    {
        return await _sFTPRepository.RenameFileAsync(connection, sourceFilePath, destinationFilePath, cancellationToken);
    }

    public async Task<SFTPActionDeleteResult> DeleteAsync(SFTPConnection connection, string remoteFilePath, CancellationToken cancellationToken)
    {
        return await _sFTPRepository.DeleteAsync(connection, remoteFilePath, cancellationToken);
    }
}
