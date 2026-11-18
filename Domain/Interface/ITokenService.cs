using Infrastructure.Models;

namespace Domain.Interface;

public interface ITokenService
{
    Task<string> CreateTokenAsync(User user);
    string CreateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
