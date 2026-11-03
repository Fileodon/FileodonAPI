using Infrastructure.Models.Enums;

namespace Domain.Interface;

public interface IAuthorizationPermissionService
{
    Task<bool> HasPermissionAsync(
        string userId,
        int organizationId,
        PermissionFlags permission);
}