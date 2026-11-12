namespace Controller.DTO.SFTPAction;

public record class SFTPActionUploadRequest(
    IFormFile File,
    string RemoteFilePath,
    bool OverrideExisting = false
);