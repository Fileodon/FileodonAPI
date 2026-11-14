using Renci.SshNet.Sftp;

namespace Infrastructure.DTO.SFTPAction;

public record class SFTPActionRenameResult(
    StatusCode StatusCode,
    string? ErrorMessage = null
);
