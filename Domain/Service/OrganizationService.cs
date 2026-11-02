using Domain.DTO;
using Domain.Enum;
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

    public async Task<MutationResult<Organization>> DeleteOrganizationByIdAsync(int organizationId, string userId)
    {
        MutationResult<Organization> authorization = await AuthorizeMutationAsync(organizationId, userId);
        if (authorization.Status != MutationStatus.Success)
        {
            return authorization;
        }

        Organization? deletedOrganization =
            await _unitOfWork.OrganizationRepository.DeleteByIdAsync(organizationId, userId);
        if (deletedOrganization is null)
        {
            return new MutationResult<Organization>(MutationStatus.NotFound, null);
        }

        await _unitOfWork.SaveChangesAsync();
        return new MutationResult<Organization>(MutationStatus.Success, deletedOrganization);
    }

    public async Task<Organization?> GetOrganizationByIdAsync(int organizationId, string userId)
    {
        return await _unitOfWork.OrganizationRepository.GetByIdAsync(organizationId, userId);
    }

    public async Task<MutationResult<Organization>> UpdateOrganizationAsync(
        int organizationId,
        string organizationName,
        string userId)
    {
        MutationResult<Organization> authorization = await AuthorizeMutationAsync(organizationId, userId);
        if (authorization.Status != MutationStatus.Success)
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
            return new MutationResult<Organization>(MutationStatus.NotFound, null);
        }

        await _unitOfWork.SaveChangesAsync();
        return new MutationResult<Organization>(MutationStatus.Success, updatedOrganization);
    }

    private async Task<MutationResult<Organization>> AuthorizeMutationAsync(
        int organizationId,
        string userId)
    {
        Organization? membership = await _unitOfWork.OrganizationRepository.GetByIdAsync(organizationId, userId);

        if (membership is null)
        {
            return new MutationResult<Organization>(MutationStatus.NotFound, null);
        }

        if (!string.Equals(
                membership.UserOwnerId,
                userId,
                StringComparison.OrdinalIgnoreCase))
        {
            return new MutationResult<Organization>(MutationStatus.Forbidden, null);
        }

        return new MutationResult<Organization>(
            MutationStatus.Success,
            membership);
    }
}
