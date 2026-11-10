using Domain.DTO.Auth;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;

namespace Domain.Interface;

public interface IAuthService
{
    Task<AuthTokens?> LoginAsync(string email, string password);
    Task<AuthTokens?> RefreshAsync(string refreshToken);
    Task LogoutAsync(string refreshToken, User user);
    Task<IdentityResult> RequestPasswordResetAsync(string email);
    Task<IdentityResult> ResetPassword(User user, string token, string newPassword);
    Task<IdentityResult> RegisterAsync(string username, string email, string password);
    Task<IdentityResult> RequestEmailConfirmationAsync(string email);
    Task<IdentityResult> ConfirmUserEmailAsync(string userId, string token);
    Task<User?> GetMeAsync(Guid id);
}
