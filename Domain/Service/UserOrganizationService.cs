using System.Linq.Expressions;
using Domain.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Domain.Service;

public class UserOrganizationService : IUserOrganizationService
{
    private readonly IUserOrganizationRepository _context;
    public UserOrganizationService(IUserOrganizationRepository context)
    {
        _context = context;
    }

    public async Task<UserOrganization?> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;
        return await _context.GetByIdAsync(id);
    }
    public async Task<IQueryable<UserOrganization>> GetAllAsync()
    {
        return await _context.GetAllAsync();
    }

    public async Task<UserOrganization> PostAsync(UserOrganization entity)
    {
        var result = await _context.PostAsync(entity);
        return result;
    }
    public async Task<UserOrganization> PutAsync(UserOrganization entity)
    {
        var result = await _context.PutAsync(entity);
        return result;
    }
    public async Task<UserOrganization?> DeleteAsync(int id)
    {
        var result = await _context.DeleteAsync(id);
        return result;
    }

    public async Task<UserOrganization> UpdateAsync(UserOrganization entity)
    {
        var result = await _context.PutAsync(entity);
        return result;
    }

    public async Task<IQueryable<UserOrganization>> WhereAsync(Expression<Func<UserOrganization, bool>> predicate, Func<IQueryable<UserOrganization>, IQueryable<UserOrganization>>? includes = null)
    {
        return await _context.WhereAsync(predicate, includes);
    }
}
