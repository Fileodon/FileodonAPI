using Renci.SshNet.Sftp;

namespace Infrastructure.Models.DTO.SFTPAction;

public record class SFTPActionDeleteResult(
    StatusCode StatusCode,
    string RemotePath,
    string? ErrorMessage = null
);