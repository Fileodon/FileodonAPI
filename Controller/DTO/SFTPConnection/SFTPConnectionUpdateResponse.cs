namespace Controller.DTO.SFTPConnection;

public record SFTPConnectionUpdateResponse(
    string? DisplayName
, int? port,
    int intermediaryPort);

public static class SFTPConnectionUpdateResponseExtensions
{
    public static SFTPConnectionUpdateResponse ToSFTPConnectionUpdateResponse(this Infrastructure.Models.SFTPConnection sftpConnection)
    {
        return new SFTPConnectionUpdateResponse(
            sftpConnection.DisplayName,
            sftpConnection.Port,
            sftpConnection.IntermediaryPort
        );
    }
}