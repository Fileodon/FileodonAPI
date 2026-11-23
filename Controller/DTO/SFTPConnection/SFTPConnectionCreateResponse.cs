namespace Controller.DTO.SFTPConnection;

public record SFTPConnectionCreateResponse(
    int Id,
    int OrganizationId,
    string? DisplayName,
    string Host,
    int? Port,
    int IntermediaryPort,
    string Status,
    string PublicKey
);

public static class SFTPConnectionCreateResponseExtensions
{
    public static SFTPConnectionCreateResponse ToDTO(this Infrastructure.Models.SFTPConnection sftpConnection)
    {
        return new SFTPConnectionCreateResponse(
            sftpConnection.Id,
            sftpConnection.OrganizationId,
            sftpConnection.DisplayName,
            sftpConnection.Host,
            sftpConnection.Port,
            sftpConnection.IntermediaryPort,
            sftpConnection.Status.ToString(),
            sftpConnection.PublicKey ?? string.Empty
        );
    }
}