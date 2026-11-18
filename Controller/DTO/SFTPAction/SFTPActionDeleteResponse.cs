using System.Text.Json.Serialization;
using Infrastructure.Models.DTO.SFTPAction;
using Renci.SshNet.Sftp;

namespace Controller.DTO.SFTPAction;

public record class SFTPActionDeleteResponse
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required StatusCode StatusCode { get; init; }
    public required List<string> DeletedItems { get; init; }
    public string? ErrorMessage { get; init; } = null;
    public string? FailedRemotePath { get; init; } = null;
}

public static class SFTPActionDeleteResponseExtensions
{
    public static SFTPActionDeleteResponse ToResponse(this SFTPActionDeleteResult result)
    {
        return new SFTPActionDeleteResponse
        {
            StatusCode = result.StatusCode,
            DeletedItems = result.DeletedItems,
            ErrorMessage = result.ErrorMessage,
            FailedRemotePath = result.FailedRemotePath
        };
    }
}