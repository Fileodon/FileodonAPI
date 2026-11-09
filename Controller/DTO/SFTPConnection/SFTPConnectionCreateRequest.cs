namespace Controller.DTO.SFTPConnection;

public record SFTPConnectionCreateRequest(
    string AuthCode,
    string DisplayName,
    string Host,
    int Port
);
