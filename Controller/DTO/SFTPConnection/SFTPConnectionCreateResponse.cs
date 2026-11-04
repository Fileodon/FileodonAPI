namespace Controller.DTO.SFTPConnection;

public record class SFTPConnectionCreateResponse(
    int Id,
    string DisplayName,
    string Host,
    int Port,
    string Status,
    string PublicKey
);

public static class SFTPConnectionCreateResponseExtensions
{
    public static SFTPConnectionCreateResponse ToDTO(this Infrastructure.Models.SFTPConnection sftpConnection)
    {
        return new SFTPConnectionCreateResponse(
            sftpConnection.Id,
            sftpConnection.DisplayName,
            sftpConnection.Host,
            sftpConnection.Port,
            sftpConnection.Status.ToString(),
            sftpConnection.PublicKey ?? string.Empty
        );
    }
}