using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class UserOrganizationRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : BaseRepository<UserOrganization>(context, transactionSettings), IUserOrganizationRepository
{
    ApplicationDbContext _context = context;
    public async Task<IEnumerable<UserOrganization>>? GetOrganizationsFromUserIdAsync(string userId)
    {
        return _context.UserOrganizations
            .AsNoTracking()
            .Where(userOrg => userOrg.UserId == userId)
            .Include(userOrg => userOrg.Organization)
            .Include(userOrg => userOrg.Role);
    }
}