using Infrastructure.Worker;

namespace Test;

public sealed class SFTPUploadJobStoreTests
{
    [Fact]
    public void Reports_aggregate_progress_and_completion()
    {
        SFTPUploadJobStore store = new();
        Guid jobId = Guid.NewGuid();
        store.Create(jobId, organizationId: 7, connectionId: 11, totalFiles: 2, totalBytes: 15, fileLengths: [10, 5]);

        store.MarkRunning(jobId);
        store.ReportProgress(jobId, fileIndex: 0, uploadedBytes: 4);
        store.MarkCompletedFile(jobId, fileIndex: 0);

        SFTPUploadJobSnapshot? progress = store.Get(jobId, organizationId: 7, connectionId: 11);

        Assert.NotNull(progress);
        Assert.Equal(SFTPUploadJobStatus.Running, progress.Status);
        Assert.Equal(1, progress.CompletedFiles);
        Assert.Equal(10, progress.UploadedBytes);
        Assert.Equal(15, progress.TotalBytes);
        Assert.Equal(66.666666, progress.ProgressPercentage, precision: 4);

        store.MarkCompleted(jobId);
        Assert.Equal(SFTPUploadJobStatus.Completed, store.Get(jobId, 7, 11)!.Status);
    }

    [Fact]
    public void Does_not_return_a_job_for_another_connection()
    {
        SFTPUploadJobStore store = new();
        Guid jobId = Guid.NewGuid();
        store.Create(jobId, organizationId: 7, connectionId: 11, totalFiles: 1, totalBytes: 5, fileLengths: [5]);

        Assert.Null(store.Get(jobId, organizationId: 7, connectionId: 12));
        Assert.Null(store.Get(jobId, organizationId: 8, connectionId: 11));
    }
}
