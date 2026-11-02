
using Domain.Interface;
using Infrastructure;
using Infrastructure.Models;
using Infrastructure.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Domain.Service
{
    public class OrganizationService : IOrganizationService
    {
        private readonly ApplicationDbContext _context;

        public OrganizationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Organization?> GetByIdAsync(int id)
        {
            if(id <= 0)
                return null;

            return await _context.Set<Organization>()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IQueryable<Organization>> GetAllAsync()
        {
            return await Task.FromResult(_context.Set<Organization>()
                .AsNoTracking()
                .AsQueryable());
        }

        public async Task<Organization> PostAsync(Organization entity)
        {
            _context.Set<Organization>().Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Organization> PutAsync(Organization entity)
        {
            _context.Set<Organization>().Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Organization> UpdateAsync(Organization entity)
        {
            _context.Set<Organization>().Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<Organization?> DeleteAsync(int id)
        {
            var organization = await _context.Set<Organization>()
                .FindAsync(id);

            if (organization == null)
                return null;

            if (await _context.Roles.AnyAsync(role => role.OrganizationId == id))
                throw new InvalidOperationException("Organization has assigned roles.");

            _context.Set<Organization>().Remove(organization);
            await _context.SaveChangesAsync();

            return organization;
        }

        public Task<IQueryable<Organization>> WhereAsync(Expression<Func<Organization, bool>> predicate, Func<IQueryable<Organization>, IQueryable<Organization>>? includes = null)
        {
            IQueryable<Organization> query = _context.Set<Organization>()
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
