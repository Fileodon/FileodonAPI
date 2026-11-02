using Domain.Interface;
using Infrastructure;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
namespace Domain.Service
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;
        public RoleService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<AspNetRole>> GetAllAsync()
        {
            return await _context.Set<AspNetRole>()
                   .AsNoTracking()
                   .Include(role => role.Organization)
                   .ToListAsync();
        }
        public async Task<AspNetRole?> GetByIdAsync(string id)
        {
            return await _context.Set<AspNetRole>()
                .AsNoTracking()
                .Include(role => role.Organization)
                .FirstOrDefaultAsync(role => role.Id == id);
        }
        public async Task<AspNetRole> CreateAsync(AspNetRole role)
        {
            _context.Set<AspNetRole>().Add(role);
            await _context.SaveChangesAsync();

            return role;
        }

        public async Task<AspNetRole?> UpdateAsync(string id, AspNetRole role)
        {
            var existingRole = await _context.Set<AspNetRole>()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingRole is null)
                return null;

            existingRole.Name = role.Name;
            existingRole.OrganizationId = role.OrganizationId;
            existingRole.PermissionMask = role.PermissionMask;

            await _context.SaveChangesAsync();

            return existingRole;
        }
        public async Task<bool> DeleteAsync(string id)
        {
            var role = await _context.Set<AspNetRole>()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role is null)
                return false;

            _context.Set<AspNetRole>().Remove(role);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
