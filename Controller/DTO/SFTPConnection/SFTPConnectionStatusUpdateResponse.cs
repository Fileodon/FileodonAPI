using Infrastructure.Models.Enums;

namespace Controller.DTO.SFTPConnection;

public record SFTPConnectionStatusUpdateResponse(
    int SFTPConnectionId,
    SFTPConnectionStatus NewStatus
);

public static class SFTPConnectionStatusUpdateResponseExtensions
{
    public static SFTPConnectionStatusUpdateResponse ToStatusUpdateResponse(this Infrastructure.Models.SFTPConnection sftpConnection)
    {
        return new SFTPConnectionStatusUpdateResponse(
            SFTPConnectionId: sftpConnection.Id,
            NewStatus: sftpConnection.Status
        );
    }
}