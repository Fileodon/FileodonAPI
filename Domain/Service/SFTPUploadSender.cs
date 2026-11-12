using Domain.Worker;
using Infrastructure.Models;

namespace Domain.Service;

public sealed record SFTPUploadJobRequest(
    SFTPConnection Connection,
    IReadOnlyList<SFTPUploadFile> Files);

public interface ISFTPUploadSender
{
    Guid Enqueue(SFTPUploadJobRequest request);
    SFTPUploadJobSnapshot? GetStatus(Guid jobId, SFTPConnection connection);
}

public sealed class SFTPUploadSender : ISFTPUploadSender
{
    private readonly ISFTPUploadQueue _queue;
    private readonly ISFTPUploadJobStore _jobStore;

    public SFTPUploadSender(ISFTPUploadQueue queue, ISFTPUploadJobStore jobStore)
    {
        _queue = queue;
        _jobStore = jobStore;
    }

    public Guid Enqueue(SFTPUploadJobRequest request)
    {
        if (request.Files.Count == 0)
        {
            throw new ArgumentException("At least one file is required.", nameof(request));
        }

        Guid jobId = Guid.NewGuid();
        _jobStore.Create(
            jobId,
            request.Connection.OrganizationId,
            request.Connection.Id,
            request.Files.Count,
            request.Files.Sum(file => file.Length),
            request.Files.Select(file => file.Length).ToArray());
        if (!_queue.Enqueue(new SFTPUploadWorkItem(jobId, request.Connection, request.Files)))
        {
            foreach (SFTPUploadFile file in request.Files)
            {
                File.Delete(file.LocalPath);
            }

            throw new InvalidOperationException("The SFTP upload queue is full.");
        }

        return jobId;
    }

    public SFTPUploadJobSnapshot? GetStatus(Guid jobId, SFTPConnection connection) =>
        _jobStore.Get(jobId, connection.OrganizationId, connection.Id);
}
