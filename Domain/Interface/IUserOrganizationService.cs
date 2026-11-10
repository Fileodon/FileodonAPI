using Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interface
{
    public interface
        IUserOrganizationService : IBaseInterface<UserOrganization>
    {
        Task<UserOrganization> CreateUserOrganizationAsync(
            string email,
            int organization_id,
            string role_id);

        Task<IQueryable<UserOrganization>> WhereAsync(
            System.Linq.Expressions.Expression<Func<UserOrganization, bool>> predicate,
            Func<IQueryable<UserOrganization>, IQueryable<UserOrganization>>? includes = null);
    }
}
