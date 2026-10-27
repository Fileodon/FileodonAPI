using Domain.Interface;
using Infrastructure;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Domain.Service
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;

        public PermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Permission?> GetByIdAsync(int id)
        {
            return await _context.Set<Permission>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<IQueryable<Permission>> GetAllAsync()
        {
            IQueryable<Permission> query = _context.Set<Permission>()
                .AsNoTracking();

            return Task.FromResult(query);
        }

        public async Task<Permission> PostAsync(Permission entity)
        {
            _context.Set<Permission>().Add(entity);

            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<Permission> PutAsync(Permission entity)
        {
            _context.Set<Permission>().Update(entity);

            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<Permission> UpdateAsync(Permission entity)
        {
            _context.Set<Permission>().Update(entity);

            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<Permission?> DeleteAsync(int id)
        {
            var permission = await _context.Set<Permission>()
                .FindAsync(id);

            if (permission is null)
                return null;

            _context.Set<Permission>().Remove(permission);

            await _context.SaveChangesAsync();

            return permission;
        }

        public Task<IQueryable<Permission>> WhereAsync(
            Expression<Func<Permission, bool>> predicate,
            Func<IQueryable<Permission>,
                 IQueryable<Permission>>? includes = null)
        {
            IQueryable<Permission> query = _context.Set<Permission>()
                .AsNoTracking()
                .Where(predicate);

            if (includes != null)
            {
                query = includes(query);
            }

            return Task.FromResult(query);
        }
    }
}