using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class RoleRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : BaseRepository<AspNetRole>(context, transactionSettings), IRoleRepository
{
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
        await context.SaveChangesAsync();
        return role;
    }
}
