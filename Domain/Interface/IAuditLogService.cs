using Infrastructure.Models;

namespace Domain.Interface;

public record class AllAuditLogsServiceReturn
{
    public IEnumerable<OrganizationAuditLog>? organizationAuditLogs { get; set; } = null!;
    public IEnumerable<SFTPConnectionAuditLog>? SFTPConnectionAuditLogs { get; set; } = null!;
    public IEnumerable<RoleAuditLog>? RoleAuditLogs { get; set; } = null!;
    public IEnumerable<UserOrganizationAuditLog>? UserOrganizationAuditLogs { get; set; } = null!;
}

public interface IAuditLogService
{
    Task<AllAuditLogsServiceReturn> GetAllAuditLogsInOrganization(int id);
    Task<IEnumerable<OrganizationAuditLog>> GetOrganizationAuditLogs(int id);
    Task<IEnumerable<SFTPConnectionAuditLog>> GetSFTPConnectionAuditLogs(int id);
    Task<IEnumerable<RoleAuditLog>> GetRoleAuditLogs(string id);
    Task<IEnumerable<UserOrganizationAuditLog>> GetUserOrganizationAuditLogs(int id);
}