using Domain.Interface;
using Infrastructure.Models;

namespace Domain.Service;

public class OrganizationService(IUnitOfWork unitOfWork) : IOrganizationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Organization> CreateOrganizationAsync(
        string organizationName,
        string workspaceURL,
        string contactEmail,
        string userId)
    {
        using IDisposable transactionScope = _unitOfWork.DisableAutomaticTransactions();
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            Organization organization = new()
            {
                UserOwnerId = userId,
                OrgName = organizationName,
                WorkspaceURL = workspaceURL,
                ContactEmail = contactEmail
            };

            organization = await _unitOfWork.OrganizationRepository.PostAsync(organization);
            await _unitOfWork.CommitAsync();

            return organization;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<OrganizationMutationResult> DeleteOrganizationByIdAsync(int organizationId, string userId)
    {
        OrganizationMutationResult authorization = await AuthorizeMutationAsync(organizationId, userId);
        if (authorization.Status != OrganizationMutationStatus.Success)
        {
            return authorization;
        }

        Organization? deletedOrganization =
            await _unitOfWork.OrganizationRepository.DeleteByIdAsync(organizationId, userId);
        if (deletedOrganization is null)
        {
            return new OrganizationMutationResult(OrganizationMutationStatus.NotFound, null);
        }

        await _unitOfWork.SaveChangesAsync();
        return new OrganizationMutationResult(OrganizationMutationStatus.Success, deletedOrganization);
    }

    public async Task<Organization?> GetOrganizationByIdAsync(int organizationId, string userId)
    {
        return await _unitOfWork.OrganizationRepository.GetByIdAsync(organizationId, userId);
    }

    public async Task<OrganizationMutationResult> UpdateOrganizationAsync(
        int organizationId,
        string organizationName,
        string userId)
    {
        OrganizationMutationResult authorization = await AuthorizeMutationAsync(organizationId, userId);
        if (authorization.Status != OrganizationMutationStatus.Success)
        {
            return authorization;
        }

        Organization? updatedOrganization =
            await _unitOfWork.OrganizationRepository.UpdateOrganizationAsync(
                organizationId,
                organizationName,
                userId);
        if (updatedOrganization is null)
        {
            return new OrganizationMutationResult(OrganizationMutationStatus.NotFound, null);
        }

        await _unitOfWork.SaveChangesAsync();
        return new OrganizationMutationResult(OrganizationMutationStatus.Success, updatedOrganization);
    }

    private async Task<OrganizationMutationResult> AuthorizeMutationAsync(
        int organizationId,
        string userId)
    {
        Organization? membership = await _unitOfWork.OrganizationRepository.GetByIdAsync(organizationId, userId);

        if (membership is null)
        {
            return new OrganizationMutationResult(OrganizationMutationStatus.NotFound, null);
        }

        if (!string.Equals(
                membership.UserOwnerId,
                userId,
                StringComparison.OrdinalIgnoreCase))
        {
            return new OrganizationMutationResult(OrganizationMutationStatus.Forbidden, null);
        }

        return new OrganizationMutationResult(
            OrganizationMutationStatus.Success,
            membership);
    }
}
