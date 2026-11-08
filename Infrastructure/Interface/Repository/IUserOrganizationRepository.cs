using Infrastructure.Models;

namespace Infrastructure.Interface.Repository;

public interface IUserOrganizationRepository : IBaseRepository<UserOrganization>
{
    Task<IEnumerable<UserOrganization>>? GetOrganizationsFromUserIdAsync(string userId);
}