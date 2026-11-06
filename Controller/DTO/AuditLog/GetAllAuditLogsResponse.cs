using Infrastructure.Models;

namespace Controller.DTO.AuditLog;

public record AuditLogDTO
(
    string Id,
    string ActorId,
    string? Action,
    string Source,
    string? oldData,
    string? newData,
    string? OrganizationId
);

public class GetAllAuditLogsResponse
{
    public IList<AuditLogDTO> auditLogs {get; set;}
    public GetAllAuditLogsResponse(
        IEnumerable<OrganizationAuditLog>? organizationAuditLogs
    )
    {
        auditLogs = new List<AuditLogDTO>();
        if (organizationAuditLogs != null)
        {
            foreach (var auditLog in organizationAuditLogs)
            {
                auditLogs.Add(new AuditLogDTO(
                    auditLog.Id.ToString(),
                    auditLog.ActorId,
                    auditLog.Action,
                    auditLog.Source,
                    auditLog.OldData,
                    auditLog.NewData,
                    auditLog.OrganizationId.ToString()
                ));
            }
        }
    }
};