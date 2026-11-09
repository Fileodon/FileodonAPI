using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class UserOrganizationRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : BaseRepository<UserOrganization>(context, transactionSettings), IUserOrganizationRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<UserOrganizationAuditLog>> GetUserOrganizationAuditLogsAsync(int userOrganizationId)
    {
        return _context.UserOrganizationAuditLogs
            .Where(audit => audit.UserOrganizationId == userOrganizationId);
    }
}