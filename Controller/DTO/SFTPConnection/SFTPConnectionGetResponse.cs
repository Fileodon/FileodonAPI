using Infrastructure.Models.Enums;

namespace Controller.DTO.SFTPConnection;

public record BaseSFTPConnectionGetResponse
{
    public required int Id { get; init; }
    public required string DisplayName { get; init; }
    public required SFTPConnectionStatus Status { get; init; }
}

public record DetailedSFTPConnectionGetResponse : BaseSFTPConnectionGetResponse
{
    public required string Host { get; init; }
    public required int Port { get; init; }
    public string? PublicKey { get; init; }
}

public static class SFTPConnectionGetResponseExtensions
{
    public static BaseSFTPConnectionGetResponse ToBaseGetResponseDTO(this Infrastructure.Models.SFTPConnection sftpConnection)
    {
        return new BaseSFTPConnectionGetResponse
        {
            Id = sftpConnection.Id,
            DisplayName = sftpConnection.DisplayName,
            Status = sftpConnection.Status,
        };
    }

    public static DetailedSFTPConnectionGetResponse ToDetailedGetResponseDTO(this Infrastructure.Models.SFTPConnection sftpConnection)
    {
        return new DetailedSFTPConnectionGetResponse
        {
            Id = sftpConnection.Id,
            DisplayName = sftpConnection.DisplayName,
            Host = sftpConnection.Host,
            Port = sftpConnection.Port,
            Status = sftpConnection.Status,
            PublicKey = sftpConnection.PublicKey
        };
    }
}