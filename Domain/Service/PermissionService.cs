using System.Linq.Expressions;
using Domain.Interface;
using Infrastructure;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;

    public PermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IQueryable<Permission>> GetAllAsync()
    {
        return await Task.FromResult(
            _context.Set<Permission>().AsNoTracking());
    }

    public async Task<Permission?> GetByIdAsync(int id)
    {
        return await _context.Set<Permission>()
            .FindAsync(id);
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
        var entity = await GetByIdAsync(id);

        if (entity is null)
            return null;

        _context.Set<Permission>().Remove(entity);
        await _context.SaveChangesAsync();

        return entity;
    }

    public async Task<IQueryable<Permission>> WhereAsync(
        Expression<Func<Permission, bool>> predicate,
        Func<IQueryable<Permission>,
            IQueryable<Permission>>? includes = null)
    {
        IQueryable<Permission> query =
            _context.Set<Permission>().Where(predicate);

        if (includes is not null)
            query = includes(query);

        return await Task.FromResult(query);
    }
}