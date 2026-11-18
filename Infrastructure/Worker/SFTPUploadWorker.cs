using System.Collections.Concurrent;
using System.Threading.Channels;
using Infrastructure.Interface.Repository.SFTP;
using Infrastructure.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Worker;

public sealed record SFTPUploadFile(
    string LocalPath,
    string RemoteFilePath,
    long Length);

public sealed record SFTPUploadInput(
    Stream Content,
    string RemoteFilePath,
    long Length);

public sealed record SFTPUploadWorkItem(
    Guid JobId,
    SFTPConnection Connection,
    IReadOnlyList<SFTPUploadFile> Files);

public enum SFTPUploadJobStatus
{
    Queued,
    Running,
    Completed,
    Failed
}

public sealed record SFTPUploadJobSnapshot(
    Guid JobId,
    SFTPUploadJobStatus Status,
    int TotalFiles,
    int CompletedFiles,
    long TotalBytes,
    long UploadedBytes,
    double ProgressPercentage,
    string? Error);

public interface ISFTPUploadJobStore
{
    void Create(Guid jobId, int organizationId, int connectionId, int totalFiles, long totalBytes, IReadOnlyList<long> fileLengths);
    void MarkRunning(Guid jobId);
    void ReportProgress(Guid jobId, int fileIndex, long uploadedBytes);
    void MarkCompletedFile(Guid jobId, int fileIndex);
    void MarkCompleted(Guid jobId);
    void MarkFailed(Guid jobId, Exception exception);
    SFTPUploadJobSnapshot? Get(Guid jobId, int organizationId, int connectionId);
}

public sealed class SFTPUploadJobStore : ISFTPUploadJobStore
{
    private sealed class JobState
    {
        public readonly object Lock = new();
        public required Guid JobId { get; init; }
        public required int OrganizationId { get; init; }
        public required int ConnectionId { get; init; }
        public required int TotalFiles { get; init; }
        public required long TotalBytes { get; init; }
        public long[] FileLengths { get; init; } = [];
        public long[] UploadedBytesByFile { get; init; } = [];
        public bool[] CompletedFiles { get; init; } = [];
        public SFTPUploadJobStatus Status { get; set; }
        public string? Error { get; set; }
    }

    private readonly ConcurrentDictionary<Guid, JobState> _jobs = new();

    public void Create(Guid jobId, int organizationId, int connectionId, int totalFiles, long totalBytes, IReadOnlyList<long> fileLengths)
    {
        if (totalFiles <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalFiles));
        }

        if (!_jobs.TryAdd(jobId, new JobState
        {
            JobId = jobId,
            OrganizationId = organizationId,
            ConnectionId = connectionId,
            TotalFiles = totalFiles,
            TotalBytes = totalBytes,
            FileLengths = [.. fileLengths],
            UploadedBytesByFile = new long[totalFiles],
            CompletedFiles = new bool[totalFiles],
            Status = SFTPUploadJobStatus.Queued
        }))
        {
            throw new InvalidOperationException($"The upload job {jobId} already exists.");
        }
    }

    public void MarkRunning(Guid jobId) => Update(jobId, state => state.Status = SFTPUploadJobStatus.Running);

    public void ReportProgress(Guid jobId, int fileIndex, long uploadedBytes)
    {
        Update(jobId, state =>
        {
            ValidateFileIndex(state, fileIndex);
            state.UploadedBytesByFile[fileIndex] = Math.Max(uploadedBytes, 0);
        });
    }

    public void MarkCompletedFile(Guid jobId, int fileIndex)
    {
        Update(jobId, state =>
        {
            ValidateFileIndex(state, fileIndex);
            state.CompletedFiles[fileIndex] = true;
            state.UploadedBytesByFile[fileIndex] = state.FileLengths[fileIndex];
        });
    }

    public void MarkCompleted(Guid jobId) => Update(jobId, state => state.Status = SFTPUploadJobStatus.Completed);

    public void MarkFailed(Guid jobId, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        Update(jobId, state =>
        {
            state.Status = SFTPUploadJobStatus.Failed;
            state.Error = exception.Message;
        });
    }

    public SFTPUploadJobSnapshot? Get(Guid jobId, int organizationId, int connectionId)
    {
        if (!_jobs.TryGetValue(jobId, out JobState? state))
        {
            return null;
        }

        lock (state.Lock)
        {
            if (state.OrganizationId != organizationId || state.ConnectionId != connectionId)
            {
                return null;
            }

            return new SFTPUploadJobSnapshot(
                state.JobId,
                state.Status,
                state.TotalFiles,
                state.CompletedFiles.Count(completed => completed),
                state.TotalBytes,
                state.UploadedBytesByFile.Sum(),
                state.TotalBytes == 0 ? 100 : state.UploadedBytesByFile.Sum() * 100d / state.TotalBytes,
                state.Error);
        }
    }

    private void Update(Guid jobId, Action<JobState> update)
    {
        if (!_jobs.TryGetValue(jobId, out JobState? state))
        {
            throw new KeyNotFoundException($"The upload job {jobId} was not found.");
        }

        lock (state.Lock)
        {
            update(state);
        }
    }

    private static void ValidateFileIndex(JobState state, int fileIndex)
    {
        if (fileIndex < 0 || fileIndex >= state.TotalFiles)
        {
            throw new ArgumentOutOfRangeException(nameof(fileIndex));
        }
    }
}

public interface ISFTPUploadQueue
{
    bool Enqueue(SFTPUploadWorkItem workItem);
}

public sealed class SFTPUploadQueue(Channel<SFTPUploadWorkItem> channel) : ISFTPUploadQueue
{
    public bool Enqueue(SFTPUploadWorkItem workItem) => channel.Writer.TryWrite(workItem);
}

public sealed class SFTPUploadWorker : BackgroundService
{
    private readonly ChannelReader<SFTPUploadWorkItem> _channelReader;
    private readonly ISFTPRepository _repository;
    private readonly ISFTPUploadJobStore _jobStore;
    private readonly ILogger<SFTPUploadWorker> _logger;

    public SFTPUploadWorker(
        Channel<SFTPUploadWorkItem> channel,
        ISFTPRepository transport,
        ISFTPUploadJobStore jobStore,
        ILogger<SFTPUploadWorker> logger)
    {
        _channelReader = channel.Reader;
        _repository = transport;
        _jobStore = jobStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (SFTPUploadWorkItem workItem in _channelReader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await _repository.UploadAsync(workItem, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _jobStore.MarkFailed(workItem.JobId, ex);
                _logger.LogError(ex, "Error occurred while processing SFTP upload job {JobId}", workItem.JobId);
            }
        }
    }
}
