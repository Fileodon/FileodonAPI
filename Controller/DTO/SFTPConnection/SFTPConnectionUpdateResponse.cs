namespace Controller.DTO.SFTPConnection;

public record SFTPConnectionUpdateResponse(
    string DisplayName
);

public static class SFTPConnectionUpdateResponseExtensions
{
    public static SFTPConnectionUpdateResponse ToSFTPConnectionUpdateResponse(this Infrastructure.Models.SFTPConnection sftpConnection)
    {
        return new SFTPConnectionUpdateResponse(
            sftpConnection.DisplayName
        );
    }
}