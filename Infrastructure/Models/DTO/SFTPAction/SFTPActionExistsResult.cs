using Renci.SshNet.Sftp;

namespace Infrastructure.Models.DTO.SFTPAction;

public record class SFTPActionExistsResult(
    StatusCode StatusCode,
    string? ErrorMessage = null,
    SftpFileAttributes? FileAttributes = null
);
