using System.Text.Json.Serialization;
using Infrastructure.Models.DTO.SFTPAction;
using Renci.SshNet.Sftp;

namespace Controller.DTO.SFTPAction;

public record class SFTPActionExistsResponse
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required StatusCode StatusCode { get; init; }
    public string? ErrorMessage { get; init; } = null;
}

public static class SFTPActionExistsResponseExtensions
{
    public static SFTPActionExistsResponse ToResponse(this SFTPActionExistsResult result)
    {
        return new SFTPActionExistsResponse
        {
            StatusCode = result.StatusCode,
            ErrorMessage = result.ErrorMessage
        };
    }
}