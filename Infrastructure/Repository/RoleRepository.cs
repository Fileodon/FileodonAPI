using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class RoleRepository : BaseRepository<AspNetRole>, IRoleRepository
{
    private readonly ApplicationDbContext _context;

    public RoleRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : base(context, transactionSettings)
    {
        _context = context;
    }

    public Task<AspNetRole?> GetByIdAsync(int organizationId, string id)
    {
        return _context.Roles
            .Where(role => role.Id == id && role.OrganizationId == organizationId)
            .FirstOrDefaultAsync();
    }

    public async Task<AspNetRole?> DeleteAsync(int organizationId, string id)
    {
        AspNetRole? role = await GetByIdAsync(organizationId, id);
        if (role is null)
        {
            return null;
        }

        _context.Roles.Remove(role);

        return role;
    }

    public async Task<IEnumerable<AspNetRole>> GetAllRolesInOrganization(int organizationId)
    {
        return _context.AspNetRoles.AsNoTracking()
            .Where(role => role.OrganizationId == organizationId);
    }

    public async Task<AspNetRole?> GetRoleAndOrganizations(string id)
    {
        return _context.AspNetRoles.AsNoTracking().Include(role => role.Organization).FirstOrDefault(role => role.Id == id);
    }

    public async Task<IEnumerable<RoleAuditLog>> GetRoleAuditLogsAsync(string roleId)
    {
        return _context.RoleAuditLogs
            .Where(audit => audit.RoleId == roleId);
    }

    public async Task<IEnumerable<long>?> GetPermissionsAsync(int orgId, string userId)
    {
        var roles = await _context.AspNetRoles
            .Where(role => role.OrganizationId == orgId)
            .ToListAsync();

        var userRoles = await _context.UserOrganizations
            .Where(userRole => userRole.UserId == userId && roles.Select(role => role.Id).Contains(userRole.RoleId))
            .ToListAsync();

        return roles
            .Where(role => userRoles.Select(userRole => userRole.RoleId).Contains(role.Id))
            .Select(role => role.PermissionMask)
            .Distinct();

    }
}
