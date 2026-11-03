using Domain.DTO;
using Domain.Enum;
using Domain.Interface;
using Infrastructure.Models;
using Infrastructure.Models.Enums;

namespace Domain.Service;

public class OrganizationService(IUnitOfWork unitOfWork, IMutationAuthorizationService<Organization> mutationAuthorizationService) : IOrganizationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMutationAuthorizationService<Organization> _mutationAuthorizationService = mutationAuthorizationService;

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

    public async Task<MutationResult<Organization>> DeleteOrganizationByIdAsync(int organizationId, string userId)
    {
        MutationResult<Organization> authorization = await _mutationAuthorizationService.AuthorizeMutationAsync(
            () => _unitOfWork.OrganizationRepository.GetByIdAsync(organizationId, userId),
            userId);
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
        MutationResult<Organization> authorization = await _mutationAuthorizationService.AuthorizeMutationAsync(
            () => _unitOfWork.OrganizationRepository.GetByIdAsync(organizationId, userId),
            userId);
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
}
