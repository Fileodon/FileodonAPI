using Domain.DTO;
using Domain.Enum;
using Domain.Interface;
using Infrastructure.Models;
using Infrastructure.Models.Enums;

namespace Domain.Service;

public class OrganizationService : IOrganizationService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrganizationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

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

            AspNetRole ownerRole = new()
            {
                Name = "Owner",
                OrganizationId = organization.Id,
                PermissionMask = (long)PermissionFlags.All
            };
            ownerRole = await _unitOfWork.RoleRepository.PostAsync(ownerRole);

            if (string.IsNullOrEmpty(ownerRole.Id))
            {
                throw new Exception("Failed to create owner role for organization.");
            }

            UserOrganization userOrganization = new()
            {
                UserId = userId,
                RoleId = ownerRole.Id,
                OrganizationId = organization.Id
            };
            await _unitOfWork.UserOrganizationRepository.PostAsync(userOrganization);

            await _unitOfWork.CommitAsync();

            return organization;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<MutationEnvelope<Organization>> DeleteOrganizationByIdAsync(int organizationId, string userId)
    {
        Organization? organization = await _unitOfWork.OrganizationRepository.GetByIdAsync(organizationId, userId);

        if (organization is null)
        {
            return new MutationEnvelope<Organization>(MutationStatus.NotFound, null);
        }

        if (!string.Equals(organization.UserOwnerId, userId, StringComparison.InvariantCultureIgnoreCase))
        {
            return new MutationEnvelope<Organization>(MutationStatus.Forbidden, null);
        }

        Organization? deletedOrganization =
            await _unitOfWork.OrganizationRepository.DeleteByIdAsync(organizationId, userId);
        if (deletedOrganization is null)
        {
            return new MutationEnvelope<Organization>(MutationStatus.NotFound, null);
        }

        await _unitOfWork.SaveChangesAsync();
        return new MutationEnvelope<Organization>(MutationStatus.Success, deletedOrganization);
    }

    public async Task<Organization?> GetOrganizationByIdAsync(int organizationId, string userId)
    {
        return await _unitOfWork.OrganizationRepository.GetByIdAsync(organizationId, userId);
    }

    public async Task<MutationEnvelope<Organization>> UpdateOrganizationAsync(
        int organizationId,
        string orgName,
        string workspaceURL,
        string contactEmail,
        string userId)
    {
        Organization? organization = await _unitOfWork.OrganizationRepository.GetByIdAsync(organizationId, userId);
        if (organization is null)
        {
            return new MutationEnvelope<Organization>(MutationStatus.NotFound, null);
        }
        if (organization.UserOwnerId != userId)
        {
            return new MutationEnvelope<Organization>(MutationStatus.Forbidden, null);
        }
        organization.OrgName = orgName;
        organization.WorkspaceURL = workspaceURL;
        organization.ContactEmail = contactEmail;

        using IDisposable transactionScope = _unitOfWork.DisableAutomaticTransactions();
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            organization = await _unitOfWork.OrganizationRepository.PutAsync(organization);
            if (organization is null)
            {
                return new MutationEnvelope<Organization>(MutationStatus.NotFound, null);
            }
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        return new MutationEnvelope<Organization>(MutationStatus.Success, organization);
    }
}
