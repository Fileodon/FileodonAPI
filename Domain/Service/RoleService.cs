using Domain.Interface;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Domain.Service
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        public RoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<AspNetRole>> GetAllAsync()
        {
            IQueryable<AspNetRole> roles = await _unitOfWork.RoleRepository.GetAllAsync();
            return await roles.AsNoTracking().ToListAsync();
        }
        public async Task<AspNetRole?> GetByIdAsync(string id)
        {
            return await _unitOfWork.RoleRepository.GetByIdAsync(id);
        }
        public async Task<AspNetRole> CreateAsync(AspNetRole role)
        {
            return await _unitOfWork.RoleRepository.PostAsync(role);
        }

        public async Task<AspNetRole?> UpdateAsync(string id, AspNetRole role)
        {
            AspNetRole? existingRole = await _unitOfWork.RoleRepository.GetByIdAsync(id);

            if (existingRole is null)
                return null;

            existingRole.Name = role.Name;
            existingRole.OrganizationId = role.OrganizationId;
            existingRole.PermissionMask = role.PermissionMask;
            await _unitOfWork.SaveChangesAsync();

            return existingRole;
        }
        public async Task<bool> DeleteAsync(string id)
        {
            AspNetRole? role = await _unitOfWork.RoleRepository.GetByIdAsync(id);

            if (role is null)
                return false;

            await _unitOfWork.RoleRepository.DeleteAsync(id);

            return true;
        }
    }
}
