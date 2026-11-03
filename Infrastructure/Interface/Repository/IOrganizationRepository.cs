using Infrastructure.Models;

namespace Infrastructure.Interface.Repository;

public interface IOrganizationRepository : IBaseRepository<Organization>
{
    Task<Organization?> GetByIdAsync(int organizationId, string userId);
    Task<Organization?> DeleteByIdAsync(int organizationId, string userId);
    Task<Organization?> UpdateOrganizationAsync(int organizationId, string newOrganizationName, string userId);
}