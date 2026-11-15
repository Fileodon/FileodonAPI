using System.Text.Json.Serialization;
using Infrastructure.Models.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Controller.DTO.SFTPConnection;

public record SFTPConnectionPollResponse
{

    public string? PublicKey { get; init; }
    public int Status { get; init; }
}

public static class SFTPConnectionPollResponseExtensions
{
    public static SFTPConnectionPollResponse toPollResponse(this Infrastructure.Models.SFTPConnection sftpConnection)
    {
        return new SFTPConnectionPollResponse
        {
            PublicKey = sftpConnection.PublicKey,
            Status = (int)sftpConnection.Status
        };
    }
}