using System.Text;
using Domain.DTO.Auth;
using Domain.Interface;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;

namespace Domain.Service;

public class AuthService : IAuthService
{
    private readonly TimeSpan RefreshTokenLifetime;
    private readonly IConfiguration _configuration;
    private readonly IEmailSender<User> _emailSender;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IConfiguration configuration,
        IEmailSender<User> emailSender,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ITokenService tokenService,
        IUnitOfWork unitOfWork
        )
    {
        _configuration = configuration;
        _emailSender = emailSender;
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        RefreshTokenLifetime = TimeSpan.FromDays(Convert.ToDouble(configuration["Jwt:RefreshTokenValidityInDays"]));
    }

    public async Task<User> GetMeAsync(Guid id)
    {
        return await _userManager.FindByIdAsync(id.ToString()) ?? throw new InvalidOperationException("User not found.");
    }

    public async Task<AuthTokens?> LoginAsync(string email, string password)
    {
        User? user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return null;
        }

        SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, password, true);
        if (!result.Succeeded)
        {
            return null;
        }

        AuthTokens tokens = await IssueTokensAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return tokens;
    }

    public async Task LogoutAsync(string refreshToken, User user)
    {
        string tokenHash = _tokenService.HashRefreshToken(refreshToken);
        RefreshToken? storedToken = await _unitOfWork.RefreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (storedToken is not null && storedToken.UserId == user.Id)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<AuthTokens?> RefreshAsync(string refreshToken)
    {
        string tokenHash = _tokenService.HashRefreshToken(refreshToken);
        RefreshToken? storedToken = await _unitOfWork.RefreshTokenRepository.GetByTokenHashAsync(tokenHash, includeUser: true);

        if (storedToken is null ||
            storedToken.RevokedAt is not null ||
            storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        storedToken.RevokedAt = DateTime.UtcNow;
        AuthTokens tokens = await IssueTokensAsync(storedToken.User);
        storedToken.ReplacedByTokenHash = _tokenService.HashRefreshToken(tokens.RefreshToken);
        await _unitOfWork.SaveChangesAsync();
        return tokens;
    }

    public async Task<IdentityResult> RegisterAsync(string username, string email, string password)
    {
        User user = new()
        {
            UserName = username,
            Email = email
        };
        // UserManager håndtere selv validering af passwords internt, som er sat i Program.cs via AddIdentityCore
        IdentityResult result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await SendConfirmationEmailAsync(user);
        }

        return result;
    }

    public async Task<IdentityResult> ResetPassword(User user, string oldPassword, string newPassword)
    {
        // UserManager håndtere selv validering af passwords internt, som er sat i Program.cs via AddIdentityCore
        return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
    }

    public async Task<IdentityResult> ConfirmUserEmailAsync(string userId, string token)
    {
        User user = await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException("User not found.");
        return await _userManager.ConfirmEmailAsync(user, token);
    }

    private async Task<AuthTokens> IssueTokensAsync(User user)
    {
        string refreshToken = _tokenService.CreateRefreshToken();
        DateTime expiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime);

        await _unitOfWork.RefreshTokenRepository.AddAsync(new RefreshToken
        {
            TokenHash = _tokenService.HashRefreshToken(refreshToken),
            UserId = user.Id,
            ExpiresAt = expiresAt
        });

        return new AuthTokens(
            await _tokenService.CreateTokenAsync(user),
            refreshToken,
            new DateTimeOffset(expiresAt, TimeSpan.Zero));
    }

    private async Task SendConfirmationEmailAsync(User user)
    {
        if (string.IsNullOrEmpty(user.Email))
        {
            throw new InvalidOperationException("User email is not set.");
        }

        string host = _configuration["API:HOST"] ?? throw new InvalidOperationException("API:HOST is not set in configuration.");

        string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        string link = $"{host}/confirm-email?userId={user.Id}&token={token}";

        await _emailSender.SendConfirmationLinkAsync(user, user.Email, link);
    }
}
