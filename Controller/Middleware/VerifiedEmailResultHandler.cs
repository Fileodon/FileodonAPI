using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Controller.Middleware;

public class VerifiedEmailResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public async Task HandleAsync(RequestDelegate next,
                                HttpContext context,
                                AuthorizationPolicy policy,
                                PolicyAuthorizationResult result)
    {
        if (result.Forbidden &&
            result.AuthorizationFailure?.FailedRequirements.OfType<EmailVerifiedRequirement>().Any() == true)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "email_not_verified" });
            return;
        }

        await _default.HandleAsync(next, context, policy, result);
    }
}
