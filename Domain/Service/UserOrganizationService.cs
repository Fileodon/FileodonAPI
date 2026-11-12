using System.Linq.Expressions;
using Domain.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;

namespace Domain.Service;

public class UserOrganizationService : IUserOrganizationService
{
    private readonly IUnitOfWork _unit;
    private readonly IUserOrganizationRepository _userOrgRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly UserManager<User> _userManager;
    public UserOrganizationService(IUnitOfWork unit, IUserOrganizationRepository userOrgRepository, IRoleRepository roleRepository, UserManager<User> userManager)
    {
        _unit = unit;
        _userOrgRepository = userOrgRepository;
        _roleRepository = roleRepository;
        _userManager = userManager;
    }

    public async Task<UserOrganization?> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;
        return await _userOrgRepository.GetByIdWithRelationshipsAsync(id);
    }
    public async Task<UserOrganization?> DeleteAsync(int id)
    {
        var userOrganization = await _userOrgRepository.GetByIdAsync(id);
        if (userOrganization == null)
        {
            throw new InvalidOperationException("UserOrganization not found");
        }
        return await _userOrgRepository.DeleteAsync(userOrganization.Id);
    }

    public async Task<UserOrganization> CreateUserOrganizationAsync(
       string email,
       int organization_id, 
       string role_id)
    {
        User? user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            throw new InvalidOperationException("User not found");

        await _unit.BeginTransactionAsync();

        try
        {
            UserOrganization userOrganization = new()
            {
                UserId = user.Id,
                OrganizationId = organization_id,
                RoleId = role_id
            };

            userOrganization = await _unit.UserOrganizationRepository.PostAsync(userOrganization);
            await _unit.CommitAsync();

            return userOrganization;
        }
        catch
        {
            await _unit.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<UserOrganization>> GetByOrganizationIdAsync(int organizationId)
    {
        var userOrganizations = await _userOrgRepository.GetUserOrganizationsByOrganizationIdAsync(organizationId);
        return userOrganizations;
    }
    public async Task<IEnumerable<UserOrganization>> GetByUserIdAsync(string userId)
    {
        return await _userOrgRepository.GetUserOrganizationsByUseridAsync(userId);
    }

    public async Task<UserOrganization> EditUserOrganizationAsync(
        int id,
        int organization_id,
        string role_id
    )
    {
        var userOrganization = await _userOrgRepository.GetByIdAsync(id);
        if (userOrganization == null)
        {
            throw new InvalidOperationException("UserOrganization not found");
        }
        var role = await _roleRepository.GetByIdAsync(organization_id, role_id);
        if(role == null)
        {
            throw new InvalidOperationException("Role not found");
        }
        userOrganization.RoleId = role_id;
        var updatedUserOrganization = await _userOrgRepository.PutAsync(userOrganization);
        userOrganization = await _userOrgRepository.GetByIdWithRelationshipsAsync(updatedUserOrganization.Id);
        if (userOrganization == null)
        {
            throw new InvalidOperationException("Could not find User organization after update");
        }
        return userOrganization;
    }
}
