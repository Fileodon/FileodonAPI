using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Infrastructure.Repository;

public class PermissionRepository : BaseRepository<Permission>, IPermissionRepository
{
    private readonly ApplicationDbContext _context;
    public PermissionRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : base(context, transactionSettings)
    {
        _context = context;
    }
    public Task<IQueryable<Permission>> WhereAsync(
    Expression<Func<Permission, bool>> predicate,
    Func<IQueryable<Permission>, IQueryable<Permission>>? includes = null)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        IQueryable<Permission> query = _context
            .Set<Permission>()
            .Where(predicate);

        if (includes is not null)
        {
            query = includes(query);
        }

        return Task.FromResult(query);
    }
}