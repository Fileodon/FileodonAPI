using Infrastructure.Models.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Middleware.Attribute;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class PermissionAttribute : AuthorizeAttribute
{
    public PermissionFlags Permission { get; }
    public PermissionAttribute(PermissionFlags permission)
    {
        Permission = permission;
        Policy = $"Permission:{(int)permission}";
    }
}
