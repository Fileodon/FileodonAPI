using Renci.SshNet.Sftp;

namespace Infrastructure.Models.DTO.SFTPAction;

public record class SFTPActionDeleteResult(
    StatusCode StatusCode,
    List<string> DeletedItems,
    string? ErrorMessage = null,
    string? FailedRemotePath = null
);