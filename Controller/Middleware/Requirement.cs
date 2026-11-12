using Infrastructure.Models.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Middleware;

public class Requirement : IAuthorizationRequirement
{
    public PermissionFlags Permission { get; }
    public Requirement(PermissionFlags permission)
    {
        Permission = permission;
    }
}
