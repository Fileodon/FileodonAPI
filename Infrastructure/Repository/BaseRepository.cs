using System.Linq.Expressions;
using Infrastructure.Interface.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;

    // Expose only the DbSet<T> to derived repositories
    protected DbSet<T> DbSet => _context.Set<T>();
    public BaseRepository(ApplicationDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public virtual async Task<T?> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is null) return null;
        DbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual Task<IQueryable<T>> GetAllAsync() => Task.FromResult<IQueryable<T>>(DbSet);

    public virtual async Task<T?> GetByIdAsync(int id) => await DbSet.FindAsync(id);

    public virtual async Task<T> PostAsync(T entity)
    {
        DbSet.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> PutAsync(T entity)
    {
        await DbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        DbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<IQueryable<T>> WhereAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? include = null)
    {
        var q = DbSet.Where(predicate);
        if (include != null) q = include(q);
        return q;
    }
}