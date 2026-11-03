using System.Linq.Expressions;
using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly IRepositoryTransactionSettings _transactionSettings;

    // Expose only the DbSet<T> to derived repositories
    protected DbSet<T> DbSet => _context.Set<T>();
    public BaseRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(transactionSettings);
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _transactionSettings = transactionSettings;
    }

    public virtual async Task<T?> DeleteAsync(int id)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            var entity = await GetByIdAsync(id);
            if (entity is null) return null;
            DbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return entity;
        });
    }

    public virtual Task<IQueryable<T>> GetAllAsync() => Task.FromResult<IQueryable<T>>(DbSet);

    public virtual async Task<T?> GetByIdAsync(int id) => await DbSet.FindAsync(id);

    public virtual async Task<T> PostAsync(T entity)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            DbSet.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        });
    }

    public virtual async Task<T> PutAsync(T entity)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            await DbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        });
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            DbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        });
    }

    private async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation)
    {
        if (!_transactionSettings.AutomaticTransactionsEnabled ||
            _context.Database.CurrentTransaction is not null)
        {
            return await operation();
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = await operation();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}