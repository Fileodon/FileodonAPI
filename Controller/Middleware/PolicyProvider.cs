using Infrastructure.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Controller.Middleware;

public class PermissionPolicyProvider
    : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(
        IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(
        string policyName)
    {
        const string prefix = "Permission:";

        if (!policyName.StartsWith(
            prefix,
            StringComparison.Ordinal))
        {
            return await base.GetPolicyAsync(policyName);
        }

        var value = policyName[prefix.Length..];

        if (!int.TryParse(value, out var permissionValue))
        {
            return null;
        }

        var permission = (PermissionFlags)permissionValue;

        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new Requirement(permission))
            .Build();

        return policy;
    }
}
