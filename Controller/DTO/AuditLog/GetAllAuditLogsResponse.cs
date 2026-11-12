using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Controller.DTO.AuditLog;

public record AuditLogDTO
(
    Guid Id,
    DateTime Timestamp,
    string ActorName,
    string? Action,
    string Source,
    string? OldData,
    string? NewData,
    int? OrganizationId = null,
    int? SFTPConnectionId = null,
    string? RoleId = null,
    int? UserOrganizationId = null
);

public class GetAllAuditLogsResponse
{
    public int total { get; set; } = 0;
    public IList<AuditLogDTO> auditLogs { get; set; }
    public GetAllAuditLogsResponse(
        IEnumerable<OrganizationAuditLog>? organizationAuditLogs = null,
        IEnumerable<SFTPConnectionAuditLog>? sFTPConnectionAuditLogs = null,
        IEnumerable<UserOrganizationAuditLog>? userOrganizationAuditLogs = null,
        IEnumerable<RoleAuditLog>? roleAuditLogs = null
    )
    {
        auditLogs = new List<AuditLogDTO>();
        if (organizationAuditLogs != null)
        {
            foreach (var auditLog in organizationAuditLogs)
            {
                auditLogs.Add(FormatBaseAuditLog(auditLog) with
                {
                    UserOrganizationId = auditLog.OrganizationId
                });
            }
            total += organizationAuditLogs.Count();
        }
        if (sFTPConnectionAuditLogs != null)
        {
            foreach (var auditLog in sFTPConnectionAuditLogs)
            {
                auditLogs.Add(FormatBaseAuditLog(auditLog) with
                {
                    SFTPConnectionId = auditLog.SFTPConnectionID
                });
            }
            total += sFTPConnectionAuditLogs.Count();
        }
        if (userOrganizationAuditLogs != null)
        {
            foreach (var auditLog in userOrganizationAuditLogs)
            {
                auditLogs.Add(FormatBaseAuditLog(auditLog) with
                {
                    UserOrganizationId = auditLog.UserOrganizationId
                });
            }
            total += userOrganizationAuditLogs.Count();
        }
        if (roleAuditLogs != null)
        {
            foreach (var auditLog in roleAuditLogs)
            {
                auditLogs.Add(FormatBaseAuditLog(auditLog) with
                {
                    RoleId = auditLog.RoleId
                });
            }
            total += roleAuditLogs.Count();
        }
    }

    private AuditLogDTO FormatBaseAuditLog(BaseAuditLog auditLog)
    {
        return new AuditLogDTO(
            Id: auditLog.Id,
            Timestamp: auditLog.Timestamp,
            ActorName: auditLog.Actor.UserName ?? "unknown",
            Action: auditLog.Action,
            Source: auditLog.Source,
            OldData: auditLog.OldData,
            NewData: auditLog.NewData
        );
    }
};