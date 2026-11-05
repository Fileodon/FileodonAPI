using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;


public class UserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("id"); // Får id claim fra JWT
        return userIdClaim?.Value;
    }
}