using System.Text.Json;
using Domain.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Interface.Repository.SFTP;
using Infrastructure.Interface.Service;
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
    private readonly IUserContextService _userContextService;
    private readonly ISFTPActionAuditLogRepository _sFTPActionAuditLogRepository;

    public SFTPActionService(
        ILogger<SFTPActionService> logger,
        ISFTPRepository sFTPRepository,
        ISFTPUploadSender uploadSender,
        ISFTPDistributionJobRepository distributionJobRepository,
        IUserContextService userContextService,
        ISFTPActionAuditLogRepository sFTPActionAuditLogRepository)
    {
        _logger = logger;
        _sFTPRepository = sFTPRepository;
        _uploadSender = uploadSender;
        _distributionJobRepository = distributionJobRepository;
        _userContextService = userContextService;
        _sFTPActionAuditLogRepository = sFTPActionAuditLogRepository;
    }

    public async Task<Guid> CreateDistributionJobAsync(
        int organizationId,
        string userId,
        IReadOnlyList<SFTPDistributionLocation> locations,
        IReadOnlyList<SFTPUploadInput> files,
        CancellationToken cancellationToken)
    {
        ISFTPDistributionJobRepository distributionJobRepository = _distributionJobRepository
            ?? throw new InvalidOperationException("The distribution job repository is required.");
        if (locations.Count == 0)
            throw new ArgumentException("At least one destination is required.", nameof(locations));
        if (files.Count == 0)
            throw new ArgumentException("At least one file is required.", nameof(files));

        string directory = Path.Combine(GetDataDirectory(), "sftp-distribution");
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
            List<SFTPDistributionDestinationProgress> progress = [.. destinations.Select(destination =>
                new SFTPDistributionDestinationProgress(
                    destination.SftpConnectionId,
                    SFTPDistributionDestinationStatus.Pending,
                    0,
                    0,
                    null))];
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
                DestinationsJson = JsonSerializer.Serialize(destinations),
                ProgressJson = JsonSerializer.Serialize(progress)
            };

            await distributionJobRepository.CreateAndMarkReadyAsync(job, cancellationToken);
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
        SFTPActionAuditLog auditLog = CreateAuditLog(connection.Id, SFTPActionType.Download, oldData: remoteFilePath);
        _sFTPActionAuditLogRepository.AddSFTPActionAuditLog(auditLog);
        return _sFTPRepository.DownloadFileAsync(connection, remoteFilePath, cancellationToken);
    }

    public Task<List<SFTPActionCreateDirectoryResult>> CreateDirectoryAsync(SFTPConnection connection, string remoteDirectoryPath, CancellationToken cancellationToken)
    {
        SFTPActionAuditLog auditLog = CreateAuditLog(connection.Id, SFTPActionType.CreateDirectory, newData: remoteDirectoryPath);
        _sFTPActionAuditLogRepository.AddSFTPActionAuditLog(auditLog);
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

        string directory = Path.Combine(GetDataDirectory(), "sftp");
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
                SFTPActionAuditLog auditLog = CreateAuditLog(connection.Id, SFTPActionType.Upload, newData: file.RemoteFilePath);
                await _sFTPActionAuditLogRepository.AddSFTPActionAuditLog(auditLog);
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
        SFTPActionAuditLog auditLog = CreateAuditLog(connection.Id, SFTPActionType.CheckExists, oldData: remoteFilePath);
        await _sFTPActionAuditLogRepository.AddSFTPActionAuditLog(auditLog);
        return await _sFTPRepository.FileExistsAsync(connection, remoteFilePath, cancellationToken);
    }

    public SFTPUploadJobSnapshot? GetUploadStatus(Guid jobId, SFTPConnection connection) =>
        _uploadSender.GetStatus(jobId, connection);

    private static string GetDataDirectory() =>
        Environment.GetEnvironmentVariable("FILEODON_DATA_DIRECTORY")
        ?? Path.Combine(AppContext.BaseDirectory, "data");

    public async Task<SFTPActionListResult> ListFilesAsync(SFTPConnection connection, string remoteDirectoryPath, CancellationToken cancellationToken)
    {
        SFTPActionAuditLog auditLog = CreateAuditLog(connection.Id, SFTPActionType.ListFiles, remoteDirectoryPath);
        await _sFTPActionAuditLogRepository.AddSFTPActionAuditLog(auditLog);
        return await _sFTPRepository.ListFilesAsync(connection, remoteDirectoryPath, cancellationToken);
    }

    public async Task<SFTPActionRenameResult> RenameFileAsync(SFTPConnection connection, string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken)
    {
        SFTPActionAuditLog auditLog = CreateAuditLog(connection.Id, SFTPActionType.Rename, sourceFilePath, destinationFilePath);
        await _sFTPActionAuditLogRepository.AddSFTPActionAuditLog(auditLog);
        return await _sFTPRepository.RenameFileAsync(connection, sourceFilePath, destinationFilePath, cancellationToken);
    }

    public async Task<SFTPActionDeleteResult> DeleteAsync(SFTPConnection connection, string remoteFilePath, CancellationToken cancellationToken)
    {
        SFTPActionAuditLog auditLog = CreateAuditLog(connection.Id, SFTPActionType.Delete, oldData: remoteFilePath);
        await _sFTPActionAuditLogRepository.AddSFTPActionAuditLog(auditLog);
        return await _sFTPRepository.DeleteAsync(connection, remoteFilePath, cancellationToken);
    }



    private SFTPActionAuditLog CreateAuditLog(int connectionId, SFTPActionType actionType, string? oldData = null, string? newData = null)
    {
        string userId = _userContextService.GetCurrentUserId() ?? throw new InvalidOperationException("User ID is required to create an audit log.");
        string source = _userContextService.getUserAgent() ?? "Unkown";

        return new SFTPActionAuditLog
        {
            SFTPConnectionID = connectionId,
            Action = actionType.ToString(),
            OldData = oldData,
            NewData = newData,
            ActorId = userId,
            Source = source,
            Timestamp = DateTime.UtcNow
        };
    }
}
