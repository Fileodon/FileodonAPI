using Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Infrastructure.Interface.Repository
{
    public interface IPermissionRepository : IBaseRepository<Permission>
    {
        Task<IQueryable<Permission>> WhereAsync(
        Expression<Func<Permission, bool>> predicate,
        Func<IQueryable<Permission>, IQueryable<Permission>>? includes = null);
    }

}
