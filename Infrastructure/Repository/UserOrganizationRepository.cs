using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repository;

public class UserOrganizationRepository : BaseRepository<UserOrganization>, IUserOrganizationRepository
{
    private readonly ApplicationDbContext _context;
    public UserOrganizationRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : base(context, transactionSettings)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserOrganizationAuditLog>> GetUserOrganizationAuditLogsAsync(int userOrganizationId)
    {
        return _context.UserOrganizationAuditLogs
            .Where(audit => audit.UserOrganizationId == userOrganizationId);
    }

    public async Task<IEnumerable<UserOrganization>> GetUserOrganizationsByOrganizationIdAsync(int organizationId)
    {
        return _context.UserOrganizations
            .Include(uo => uo.Role)
            .Include(uo => uo.User)
            .Where(uo => uo.OrganizationId == organizationId);
    }
    public async Task<IEnumerable<UserOrganization>> GetUserOrganizationsByUseridAsync(string userId)
    {
        return _context.UserOrganizations
            .Include(uo => uo.Role)
            .Include(uo => uo.User)
            .Include(uo => uo.Organization)
            .Where(uo => uo.UserId == userId);
    }

    public async Task<UserOrganization?> GetByIdWithRelationshipsAsync(int id) 
    {
        return await _context.UserOrganizations
            .Include(uo => uo.Role)
            .Include(uo => uo.User)
            .FirstAsync(uo => uo.Id == id);
    }

}