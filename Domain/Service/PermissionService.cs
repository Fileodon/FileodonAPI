using System.Linq.Expressions;
using Domain.Interface;
using Domain.Service;
using Infrastructure;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PermissionService(IPermissionRepository permissionRepository, IUnitOfWork unitOfWork)
    {
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IQueryable<Permission>> GetAllAsync()
    {
        return await _permissionRepository.GetAllAsync();
    }

    public async Task<Permission?> GetByIdAsync(int id)
    {
        return await _permissionRepository.GetByIdAsync(id);
    }

    public async Task<Permission> PostAsync(Permission entity)
    {
        return await _permissionRepository.PostAsync(entity);
    }

    public async Task<Permission> PutAsync(Permission entity)
    {
        return await _permissionRepository.PutAsync(entity);
    }

    public async Task<Permission> UpdateAsync(Permission entity)
    {
        return await _permissionRepository.UpdateAsync(entity);
    }

    public async Task<Permission?> DeleteAsync(int id)
    {
        return await _permissionRepository.DeleteAsync(id);

    }

    public async Task<IQueryable<Permission>> WhereAsync(
        Expression<Func<Permission, bool>> predicate,
        Func<IQueryable<Permission>, IQueryable<Permission>>? includes = null)
    {
        return await _permissionRepository.WhereAsync(predicate, includes);
    }
}