using Renci.SshNet.Sftp;

namespace Controller.DTO.SFTPAction;

public record class SFTPActionListFilesResponse
(
    string AbsoluteName,
    string RelativeName,
    DateTime LastAccessTime,
    DateTime LastWriteTime,
    long Size,
    bool IsDirectory
);

public static class SFTPActionListFilesResponseExtensions
{
    public static SFTPActionListFilesResponse FromSftpFile(this ISftpFile sftpFile)
    {
        return new SFTPActionListFilesResponse(
            AbsoluteName: sftpFile.FullName,
            RelativeName: sftpFile.Name,
            LastAccessTime: sftpFile.LastAccessTime,
            LastWriteTime: sftpFile.LastWriteTime,
            Size: sftpFile.Length,
            IsDirectory: sftpFile.IsDirectory
        );
    }
}
