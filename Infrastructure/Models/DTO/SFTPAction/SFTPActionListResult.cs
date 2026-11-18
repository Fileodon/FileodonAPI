using Renci.SshNet.Sftp;

namespace Infrastructure.Models.DTO.SFTPAction;

public record class SFTPActionListResult(
    List<ISftpFile> SftpFiles,
    StatusCode StatusCode,
    string? ErrorMessage = null
);