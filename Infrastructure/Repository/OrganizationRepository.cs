using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class OrganizationRepository : BaseRepository<Organization>, IOrganizationRepository
{
    private readonly ApplicationDbContext _context;

    public OrganizationRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : base(context, transactionSettings)
    {
        _context = context;
    }

    /// <summary>
    /// Deletes an organization by its ID and verifies that the specified user is associated with that organization.
    /// </summary>
    /// <param name="organizationId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <remarks>Changes are not automatically saved to the database. Call <see cref="IUnitOfWork.SaveChangesAsync"/> to persist changes.</remarks>
    public async Task<Organization?> DeleteByIdAsync(int organizationId, string userId)
    {

        var organization = await _context.Organizations
            .Include(o => o.UserOrganizations)
            .FirstOrDefaultAsync(o => o.Id == organizationId && o.UserOrganizations.Any(uo => uo.UserId == userId));

        if (organization == null)
        {
            return null;
        }

        _context.Organizations.Remove(organization);

        return organization;
    }

    /// <summary>
    /// Gets an organization by its ID and verifies that the specified user is associated with that organization.
    /// </summary>
    /// <param name="organizationId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <remarks>Changes are not automatically saved to the database. Call <see cref="IUnitOfWork.SaveChangesAsync"/> to persist changes.</remarks>
    public async Task<Organization?> GetByIdAsync(int organizationId, string userId)
    {
        return await _context.Organizations
            .Include(o => o.UserOrganizations)
            .ThenInclude(uo => uo.User)
            .FirstOrDefaultAsync(o => o.Id == organizationId && o.UserOrganizations.Any(uo => uo.UserId == userId));
    }

    public async Task<Organization?> UpdateOrganizationAsync(int organizationId, string newOrganizationName, string userId)
    {
        var organization = await _context.Organizations
            .Include(o => o.UserOrganizations)
            .FirstOrDefaultAsync(o => o.Id == organizationId && o.UserOrganizations.Any(uo => uo.UserId == userId));

        if (organization == null)
        {
            return null;
        }

        organization.OrgName = newOrganizationName;
        _context.Organizations.Update(organization);

        return organization;
    }

    public async Task<IEnumerable<OrganizationAuditLog>> GetOrganizationAuditLogsAsync(int organizationId)
    {
        return _context.OrganizationAuditLogs
            .Where(audit => audit.OrganizationId == organizationId);
    }

}