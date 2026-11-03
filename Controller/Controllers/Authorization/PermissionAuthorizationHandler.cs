using Domain.Interface;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Controller.Controllers.Authorization;

//TODDO: 
//Understøtter pt. ikke forskellige rettigheder for forskellige servere indenfor samme organisation.
public class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement, int>
{
    private readonly IAuthorizationPermissionService _permissionService;
    private readonly UserManager<User> _userManager;

    public PermissionAuthorizationHandler(
        IAuthorizationPermissionService permissionService,
        UserManager<User> userManager)
    {
        _permissionService = permissionService;
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement,
        int organizationId)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Fail();
            return;
        }

        var userId = _userManager.GetUserId(context.User);

        if (string.IsNullOrWhiteSpace(userId) || organizationId <= 0)
        {
            context.Fail();
            return;
        }

        var hasPermission = await _permissionService.HasPermissionAsync(
            userId,
            organizationId,
            requirement.permissionsFlags);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}