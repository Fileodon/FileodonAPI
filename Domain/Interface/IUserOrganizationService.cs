using Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interface;
public interface
    IUserOrganizationService
{
    Task<UserOrganization?> GetByIdAsync(int id);
    Task<UserOrganization?> DeleteAsync(int id);
    Task<UserOrganization> CreateUserOrganizationAsync(
        string email,
        int organization_id,
        string role_id
    );
    Task<UserOrganization> EditUserOrganizationAsync(
        int id,
        string role_id
    );
    Task<IEnumerable<UserOrganization>> GetByOrganizationIdAsync(int organizationId);
    Task<IEnumerable<UserOrganization>> GetByUserIdAsync(string userId);
}

