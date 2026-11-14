using Renci.SshNet.Sftp;

namespace Infrastructure.Models.DTO.SFTPAction;

public record class SFTPActionCreateDirectoryResult(
    StatusCode StatusCode,
    string RemoteDirectoryPath,
    string? ErrorMessage = null
);
