using Infrastructure.Models;
using System.Linq.Expressions;

namespace Infrastructure.Interface.Repository
{
    public interface IPermissionRepository : IBaseRepository<Permission>
    {
        Task<IQueryable<Permission>> WhereAsync(
        Expression<Func<Permission, bool>> predicate,
        Func<IQueryable<Permission>, IQueryable<Permission>>? includes = null);
    }

}
