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
    int? OrganizationId,
    int? SFTPConnectionId,
    string? RoleId,
    int? UserOrganizationId
);

public class GetAllAuditLogsResponse
{
    public IList<AuditLogDTO> auditLogs {get; set;}
    public GetAllAuditLogsResponse(
        IEnumerable<OrganizationAuditLog>? organizationAuditLogs,
        IEnumerable<SFTPConnectionAuditLog>? sFTPConnectionAuditLogs,
        IEnumerable<UserOrganizationAuditLog>? userOrganizationAuditLogs,
        IEnumerable<RoleAuditLog>? roleAuditLogs
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
                    auditLog.OrganizationId,
                    null,
                    null,
                    null
                ));
            }
        }
        if (sFTPConnectionAuditLogs != null)
        {
            foreach (var auditLog in sFTPConnectionAuditLogs)
            {
                auditLogs.Add(new AuditLogDTO(
                    auditLog.Id.ToString(),
                    auditLog.ActorId,
                    auditLog.Action,
                    auditLog.Source,
                    auditLog.OldData,
                    auditLog.NewData,
                    null,
                    auditLog.SFTPConnectionID,
                    null,
                    null
                ));
            }
        }
        if (userOrganizationAuditLogs != null)
        {
            foreach (var auditLog in userOrganizationAuditLogs)
            {
                auditLogs.Add(new AuditLogDTO(
                    auditLog.Id.ToString(),
                    auditLog.ActorId,
                    auditLog.Action,
                    auditLog.Source,
                    auditLog.OldData,
                    auditLog.NewData,
                    null,
                    null,
                    null,
                    auditLog.UserOrganizationId
                ));
            }
        }
        if (roleAuditLogs != null)
        {
            foreach (var auditLog in roleAuditLogs)
            {
                auditLogs.Add(new AuditLogDTO(
                    auditLog.Id.ToString(),
                    auditLog.ActorId,
                    auditLog.Action,
                    auditLog.Source,
                    auditLog.OldData,
                    auditLog.NewData,
                    null,
                    null,
                    auditLog.RoleId,
                    null
                ));
            }
        }


    }
};