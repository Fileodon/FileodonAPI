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

    public Task<AspNetRole?> GetByIdAsync(string id) =>
        DbSet.SingleOrDefaultAsync(role => role.Id == id);

    public async Task<AspNetRole?> DeleteAsync(string id)
    {
        AspNetRole? role = await GetByIdAsync(id);
        if (role is null)
        {
            return null;
        }

        DbSet.Remove(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<IEnumerable<AspNetRole>> GetAllRolesAndOrganizations()
    {
        return _context.AspNetRoles.AsNoTracking().Include(role => role.Organization);
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
}
