using Renci.SshNet.Sftp;

namespace Infrastructure.Models.DTO.SFTPAction;

public record class SFTPActionRenameResult(
    StatusCode StatusCode,
    string? ErrorMessage = null
);
