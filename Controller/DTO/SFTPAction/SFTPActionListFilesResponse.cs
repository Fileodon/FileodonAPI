using Renci.SshNet.Sftp;

namespace Controller.DTO.SFTPAction;

public record class SFTPActionListFilesResponse
(
    string AbsoluteName,
    string RelativeName,
    DateTime LastAccessTime,
    DateTime LastWriteTime,
    long Size,
    bool IsDirectory,
    bool hasAccess
);

public static class SFTPActionListFilesResponseExtensions
{
    public static SFTPActionListFilesResponse FromSftpFile(ISftpFile sftpFile)
    {
        return new SFTPActionListFilesResponse(
            AbsoluteName: sftpFile.FullName,
            RelativeName: sftpFile.Name,
            LastAccessTime: sftpFile.LastAccessTime,
            LastWriteTime: sftpFile.LastWriteTime,
            Size: sftpFile.Length,
            IsDirectory: sftpFile.IsDirectory,
            hasAccess: true // Tror vi kan bruge sftp klienten til at validere om vi har adgang. Der er ikke en "currentUser" eller lign Read værdi på !SftpFile fra hvad jeg kan se
        );
    }
}
