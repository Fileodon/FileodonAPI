using Domain.DTO.Auth;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;

namespace Domain.Interface;

public interface IAuthService
{
    Task<AuthTokens?> LoginAsync(string username, string password);
    Task<AuthTokens?> RefreshAsync(string refreshToken);
    Task LogoutAsync(string refreshToken, User user);
    Task<IdentityResult> ResetPassword(User user, string oldPassword, string newPassword);
    Task<IdentityResult> RegisterAsync(string username, string email, string password);
    Task<IdentityResult> ConfirmUserEmailAsync(string userId, string token);
    Task<User> GetMeAsync(Guid id);
}
