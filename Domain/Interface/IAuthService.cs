using Infrastructure.Models;

namespace Domain.Interface;

public interface IAuthService
{
    Task<AuthTokens?> LoginAsync(string username, string password);
    Task<AuthTokens?> RefreshAsync(string refreshToken);
    Task RegisterAsync(string username, string email, string password);
    Task<User> GetMeAsync(Guid id);
}
