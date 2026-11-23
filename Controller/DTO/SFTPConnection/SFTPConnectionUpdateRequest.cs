namespace Controller.DTO.SFTPConnection;

public record SFTPConnectionUpdateRequest(
    string DisplayName,
    int? Port,
    int? IntermediaryPort
);