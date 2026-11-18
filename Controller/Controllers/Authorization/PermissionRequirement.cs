using Infrastructure.Models.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionFlags permissionsFlags { get; }
    public PermissionRequirement(PermissionFlags permissions)
    {
        permissionsFlags = permissions;
    }
}
