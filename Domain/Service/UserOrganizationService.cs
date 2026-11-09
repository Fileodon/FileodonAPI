using Domain.Interface;
using Infrastructure;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Domain.Service;

public class UserOrganizationService : IUserOrganizationService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unit;
    private readonly IUserOrganizationRepository _userOrgRepository;
    public UserOrganizationService(ApplicationDbContext context, IUnitOfWork unit, IUserOrganizationRepository userOrgRepository)
    {
        _context = context;
        _unit = unit;
        _userOrgRepository = userOrgRepository;
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
    public async Task<UserOrganization> DeleteAsync(int id)
    {
        return await _userOrgRepository.DeleteAsync(id);
        
    }

    public async Task<UserOrganization> UpdateAsync(UserOrganization entity)
    {
        return await _userOrgRepository.UpdateAsync(entity);
    }

    public Task<IQueryable<UserOrganization>> WhereAsync(Expression<Func<UserOrganization, bool>> predicate, Func<IQueryable<UserOrganization>, IQueryable<UserOrganization>>? includes = null)
    {
        IQueryable<UserOrganization> query = _context.Set<UserOrganization>()
            .AsNoTracking()
            .Where(predicate);
        if (includes != null)
        {
            query = includes(query);
        }
        return Task.FromResult(query);
    }

    public async Task<UserOrganization> CreateUserOrganizationAsync(
       string user_id,
       int organization_id, 
       string role_id)
    {
        using IDisposable transactionScope = _unit.DisableAutomaticTransactions();
        await _unit.BeginTransactionAsync();

        try
        {
            UserOrganization userOrganization = new()
            {
                UserId = user_id,
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
