using Infrastructure.Models.Enums;

namespace Controller.DTO.SFTPConnection;

public record SFTPConnectionStatusUpdateRequest(
    int SFTPConnectionId,
    SFTPConnectionStatus NewStatus,
);