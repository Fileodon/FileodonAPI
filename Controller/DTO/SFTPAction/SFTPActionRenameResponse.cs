using System.Text.Json.Serialization;
using Infrastructure.Models.DTO.SFTPAction;
using Renci.SshNet.Sftp;

namespace Controller.DTO.SFTPAction;

public record class SFTPActionRenameResponse
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required StatusCode StatusCode { get; init; }
    public string? ErrorMessage { get; init; } = null;
}

public static class SFTPActionRenameResponseExtensions
{
    public static SFTPActionRenameResponse ToResponse(this SFTPActionRenameResult renameResult)
    {
        return new SFTPActionRenameResponse()
        {
            StatusCode = renameResult.StatusCode,
            ErrorMessage = renameResult.ErrorMessage
        };
    }
}