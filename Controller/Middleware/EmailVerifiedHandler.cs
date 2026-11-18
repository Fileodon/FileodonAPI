using Controller.Extensions;
using Controller.Middleware.Attribute;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Controller.Middleware;

public class EmailVerifiedRequirement : IAuthorizationRequirement { }

public class EmailVerifiedHandler : AuthorizationHandler<EmailVerifiedRequirement>
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<EmailVerifiedHandler> _logger;
    public EmailVerifiedHandler(UserManager<User> userManager, ILogger<EmailVerifiedHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EmailVerifiedRequirement requirement)
    {
        Endpoint? endpoint = (context.Resource as HttpContext)?.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<AllowUnverifiedEmailAttribute>() != null)
        {
            _logger.LogDebug("AllowUnverifiedEmailAttribute found. Skipping email verification check.");
            context.Succeed(requirement);
            return;
        }


        if (!context.User.TryGetClaimValue("id", out var userId) || userId is null)
        {
            _logger.LogWarning("User ID claim not found.");
            context.Fail();
            return;
        }

        User? user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            _logger.LogWarning("User not found.");
            context.Fail();
            return;
        }

        if (user.EmailConfirmed)
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogDebug("User email not verified.");
        }

        return;
    }
}
