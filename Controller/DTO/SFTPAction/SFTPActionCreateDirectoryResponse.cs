using System.Text.Json.Serialization;
using Infrastructure.Models.DTO.SFTPAction;
using Renci.SshNet.Sftp;

namespace Controller.DTO.SFTPAction;

public record class SFTPActionCreateDirectoryResponse
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required StatusCode StatusCode { get; init; }
    public required string RemoteDirectoryPath { get; init; }
    public string? ErrorMessage { get; init; }
}

public static class SFTPActionCreateDirectoryResponseExtensions
{
    public static SFTPActionCreateDirectoryResponse ToResponse(this SFTPActionCreateDirectoryResult response)
    {
        return new SFTPActionCreateDirectoryResponse
        {
            StatusCode = response.StatusCode,
            RemoteDirectoryPath = response.RemoteDirectoryPath.Replace("\\", "/"),
            ErrorMessage = response.ErrorMessage
        };
    }
}