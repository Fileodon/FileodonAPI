using Infrastructure.Interface.Service;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Service;


public class UserContextService : IUserContextService
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

    public string? getUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
    }
}