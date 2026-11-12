namespace Controller.DTO.SFTPAction;

public record class SFTPActionUploadRequest(
    List<IFormFile> Files,
    string RemoteFilePaths,
    bool OverrideExisting = false
)
{
    public string[] GetRemoteFilePaths() =>
        (RemoteFilePaths ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

public record class SFTPActionUploadResponse(Guid JobId);