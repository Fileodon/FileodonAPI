using System.Security.Claims;
using Domain.Interface;
using Infrastructure.Models.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Middleware;

public class Handler : AuthorizationHandler<Requirement>
{
    private readonly IRoleService permissionService;
    public Handler(IRoleService permissionService)
    {
        this.permissionService = permissionService;
    }
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, Requirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        if (context.Resource is not HttpContext httpContext)
            return;

        var organizationId = httpContext.Request.RouteValues
            .TryGetValue("organizationId", out var value)
                ? value?.ToString()
                : null;

        if (!int.TryParse(organizationId, out var orgId))
            return;

        var permissions = await permissionService
            .GetPermissionsAsync(orgId, userId);

        if (permissions == null || !permissions.Any())
            return;

        var permissionMask = permissions.Aggregate(PermissionFlags.None, (current, permission) => current | (PermissionFlags)permission);

        if ((permissionMask & requirement.Permission)
            == requirement.Permission)
        {
            context.Succeed(requirement);
        }
    }
}
