namespace Controller.DTO.SFTPConnection;

public record class SFTPConnectionDeleteResponse
{
    public int Id { get; init; }
    public required string DisplayName { get; init; }
    public required string Host { get; init; }
    public required int Port { get; init; }
}

public static class SFTPConnectionDeleteResponseExtensions
{
    public static SFTPConnectionDeleteResponse ToSFTPConnectionDeleteResponse(this Infrastructure.Models.SFTPConnection sftpConnection)
    {
        return new SFTPConnectionDeleteResponse
        {
            Id = sftpConnection.Id,
            DisplayName = sftpConnection.DisplayName,
            Host = sftpConnection.Host,
            Port = sftpConnection.Port
        };
    }
}