using Domain.Interface;
using Infrastructure.Interface.Repository.SFTP;
using Infrastructure.Models;
using Infrastructure.Models.DTO.SFTPAction;
using Infrastructure.Worker;
using Microsoft.Extensions.Logging;

namespace Domain.Service;

public class SFTPActionService : ISFTPActionService
{
    private readonly ILogger<SFTPActionService> _logger;
    private readonly ISFTPRepository _sFTPRepository;
    private readonly ISFTPUploadSender _uploadSender;

    public SFTPActionService(
        ILogger<SFTPActionService> logger,
        ISFTPRepository sFTPRepository,
        ISFTPUploadSender uploadSender)
    {
        _logger = logger;
        _sFTPRepository = sFTPRepository;
        _uploadSender = uploadSender;
    }

    public Task<SFTPActionDownloadResult> DownloadFileAsync(SFTPConnection connection, string remoteFilePath, CancellationToken cancellationToken)
    {
        return _sFTPRepository.DownloadFileAsync(connection, remoteFilePath, cancellationToken);
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
}
