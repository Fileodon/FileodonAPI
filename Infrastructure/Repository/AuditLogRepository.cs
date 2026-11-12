using Infrastructure.Interface;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class AuditLogRepository(ApplicationDbContext context) : IAuditLogRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<OrganizationAuditLog>> GetOrganizationAuditLogsFromOrganizationId(int id)
    {
        return _context.OrganizationAuditLogs
            .Include(audit => audit.Actor)
            .Where(audit => audit.OrganizationId == id);
    }
    public async Task<IEnumerable<SFTPConnectionAuditLog>> GetSFTPConnectionAuditLogsFromOrganizationId(int id)
    {
        return _context.SFTPConnectionAuditLogs
            .Include(audit => audit.SFTPConnnection)
            .Include(audit => audit.Actor)
            .Where(audit => audit.SFTPConnnection.OrganizationId == id);
    }
    public async Task<IEnumerable<RoleAuditLog>> GetRoleAuditLogsFromOrganizationId(int id)
    {
        return _context.RoleAuditLogs
            .Include(audit => audit.Role)
            .Include(audit => audit.Actor)
            .Where(audit => audit.Role.OrganizationId == id); // Left off with this :)
    }
    public async Task<IEnumerable<UserOrganizationAuditLog>> GetUserOrganizationAuditLogsFromOrganizationId(int id)
    {
        return _context.UserOrganizationAuditLogs
            .Include(audit => audit.UserOrganization)
            .Include(audit => audit.Actor)
            .Where(audit => audit.UserOrganization.OrganizationId == id);
    }
}