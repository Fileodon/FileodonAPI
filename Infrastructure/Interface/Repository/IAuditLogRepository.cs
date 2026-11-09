using Infrastructure.Models;

namespace Infrastructure.Interface;

public interface IAuditLogRepository
{
    Task<IEnumerable<OrganizationAuditLog>> GetOrganizationAuditLogsFromOrganizationId(int id);
    Task<IEnumerable<SFTPConnectionAuditLog>> GetSFTPConnectionAuditLogsFromOrganizationId(int id);
    Task<IEnumerable<RoleAuditLog>> GetRoleAuditLogsFromOrganizationId(int id);
    Task<IEnumerable<UserOrganizationAuditLog>> GetUserOrganizationAuditLogsFromOrganizationId(int id);
}