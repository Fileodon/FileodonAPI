using System.Linq.Expressions;
using Domain.Interface;
using Infrastructure;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Domain.Service;

public class UserOrganizationService : IUserOrganizationService
{
    private readonly IUnitOfWork _unit;
    private readonly IUserOrganizationRepository _userOrgRepository;
    private readonly UserManager<User> _userManager;
    public UserOrganizationService(IUnitOfWork unit, IUserOrganizationRepository userOrgRepository, UserManager<User> userManager)
    {
        _unit = unit;
        _userOrgRepository = userOrgRepository;
        _userManager = userManager;
    }

    public async Task<UserOrganization?> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;
        return await _userOrgRepository.GetByIdAsync(id);
    }
    public async Task<IQueryable<UserOrganization>> GetAllAsync()
    {
        return await _userOrgRepository.GetAllAsync();
    }

    public async Task<UserOrganization> PostAsync(UserOrganization entity)
    {
        return await _userOrgRepository.PostAsync(entity);
    }
    public async Task<UserOrganization> PutAsync(UserOrganization entity)
    {
        return await _userOrgRepository.PutAsync(entity);
    }
    public async Task<UserOrganization?> DeleteAsync(int id)
    {
        return await _userOrgRepository.DeleteAsync(id);
        
    }

    public async Task<UserOrganization> UpdateAsync(UserOrganization entity)
    {
        return await _userOrgRepository.UpdateAsync(entity);
    }

    public Task<IQueryable<UserOrganization>> WhereAsync(
        Expression<Func<UserOrganization, bool>> predicate,
        Func<IQueryable<UserOrganization>, IQueryable<UserOrganization>>? includes = null)
    {
        return _userOrgRepository.WhereAsync(predicate, includes);
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


}
