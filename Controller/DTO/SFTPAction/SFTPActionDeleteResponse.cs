using System.Text.Json.Serialization;
using Infrastructure.Models.DTO.SFTPAction;
using Renci.SshNet.Sftp;

namespace Controller.DTO.SFTPAction;

public record class SFTPActionDeleteResponse
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required StatusCode StatusCode { get; init; }
    public string? ErrorMessage { get; init; } = null;
}

public static class SFTPActionDeleteResponseExtensions
{
    public static SFTPActionDeleteResponse ToResponse(this SFTPActionDeleteResult result)
    {
        return new SFTPActionDeleteResponse
        {
            StatusCode = result.StatusCode,
            ErrorMessage = result.ErrorMessage
        };
    }
}