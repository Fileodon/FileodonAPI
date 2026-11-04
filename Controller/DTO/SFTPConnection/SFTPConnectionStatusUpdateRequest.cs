using Infrastructure.Models.Enums;

namespace Controller.DTO.SFTPConnection;

public record class SFTPConnectionStatusUpdateRequest(
    int SFTPConnectionId,
    SFTPConnectionStatus NewStatus
);