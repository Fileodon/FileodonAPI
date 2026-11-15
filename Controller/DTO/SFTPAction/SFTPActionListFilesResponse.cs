using System.Text.Json.Serialization;
using Infrastructure.Models.DTO.SFTPAction;
using Renci.SshNet.Sftp;

namespace Controller.DTO.SFTPAction;

public record class SFTPFileInfo(
    string AbsoluteName,
    string RelativeName,
    DateTime LastAccessTime,
    DateTime LastWriteTime,
    long Size,
    bool IsDirectory,
    bool hasAccess
);

public record class SFTPActionListFilesResponse
{
    public required List<SFTPFileInfo> Files { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required StatusCode StatusCode { get; init; }
    public string? ErrorMessage { get; init; }
}

public static class SFTPActionListFilesResponseExtensions
{
    public static SFTPActionListFilesResponse FromSftpFile(this SFTPActionListResult result)
    {
        return new SFTPActionListFilesResponse
        {
            Files = [.. result.SftpFiles.Where(file => file.Name != ".").Select(file => new SFTPFileInfo(
                AbsoluteName: file.FullName,
                RelativeName: file.Name,
                LastAccessTime: file.LastAccessTime,
                LastWriteTime: file.LastWriteTime,
                Size: file.Length,
                IsDirectory: file.IsDirectory
            ))],
            StatusCode = result.StatusCode,
            ErrorMessage = result.ErrorMessage
        };
    }
}
