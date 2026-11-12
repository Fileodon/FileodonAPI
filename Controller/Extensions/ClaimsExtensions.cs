using System.Security.Claims;

namespace Controller.Extensions;

public static class ClaimsExtensions
{
    public static bool TryGetClaimValue(this ClaimsPrincipal claimsPrincipal, string claimType, out string? claimValue)
    {
        claimValue = claimsPrincipal.FindFirst(claimType)?.Value;
        return claimValue is not null;
    }
}