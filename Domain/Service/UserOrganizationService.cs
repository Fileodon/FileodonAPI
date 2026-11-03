using System.Linq.Expressions;
using Domain.Interface;
using Infrastructure;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Domain.Service;

public class UserOrganizationService : IUserOrganizationService
{
    private readonly ApplicationDbContext _context;
    public UserOrganizationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserOrganization?> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;
        return await _context.Set<UserOrganization>()
            .AsNoTracking()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
    }
    public async Task<IQueryable<UserOrganization>> GetAllAsync()
    {
        return await Task.FromResult(_context.Set<UserOrganization>()
            .AsNoTracking()
            .AsQueryable());
    }

    public async Task<UserOrganization> PostAsync(UserOrganization entity)
    {
        _context.Set<UserOrganization>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task<UserOrganization> PutAsync(UserOrganization entity)
    {
        _context.Set<UserOrganization>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task<UserOrganization?> DeleteAsync(int id)
    {
        var exsistingUserOrg = await _context.Set<UserOrganization>().FindAsync(id);
        if (exsistingUserOrg == null)
        {
            return null;
        }
        _context.Set<UserOrganization>().Remove(exsistingUserOrg);
        await _context.SaveChangesAsync();
        return exsistingUserOrg;
    }

    public async Task<UserOrganization> UpdateAsync(UserOrganization entity)
    {
        _context.Set<UserOrganization>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public Task<IQueryable<UserOrganization>> WhereAsync(Expression<Func<UserOrganization, bool>> predicate, Func<IQueryable<UserOrganization>, IQueryable<UserOrganization>>? includes = null)
    {
        IQueryable<UserOrganization> query = _context.Set<UserOrganization>()
            .AsNoTracking()
            .Where(predicate);
        if (includes != null)
        {
            query = includes(query);
        }
        return Task.FromResult(query);
    }
}
