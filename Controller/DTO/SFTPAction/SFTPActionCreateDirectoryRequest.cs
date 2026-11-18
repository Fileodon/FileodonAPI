namespace Controller.DTO.SFTPAction;

public record class SFTPActionCreateDirectoryRequest
{
    public required string RemoteDirectoryPath { get; init; }
}
