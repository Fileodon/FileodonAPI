using Infrastructure.Models;

namespace Domain.Interface;

public interface IOrganizationService
{
    Task<Organization?> GetOrganizationByIdAsync(int organizationId, string userId);
    Task<Organization> CreateOrganizationAsync(string organizationName, string workspaceURL, string contactEmail, string userId);
    Task<OrganizationMutationResult> DeleteOrganizationByIdAsync(int organizationId, string userId);
    Task<OrganizationMutationResult> UpdateOrganizationAsync(int organizationId, string organizationName, string userId);
}
