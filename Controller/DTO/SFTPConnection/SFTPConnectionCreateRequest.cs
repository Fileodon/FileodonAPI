namespace Controller.DTO.SFTPConnection;

public record class SFTPConnectionCreateRequest(
    string AuthCode,
    string DisplayName,
    string Host,
    int Port
);
