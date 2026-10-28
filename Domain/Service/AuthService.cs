using Domain.DTO.Auth;
using Domain.Interface;
using Infrastructure;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Domain.Service;

public class AuthService(
    IConfiguration configuration,
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ITokenService tokenService,
    ApplicationDbContext dbContext) : IAuthService
{
    private readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(Convert.ToDouble(configuration["Jwt:RefreshTokenValidityInDays"]));

    private readonly UserManager<User> _userManager = userManager;
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly ITokenService _tokenService = tokenService;
    private readonly ApplicationDbContext _dbContext = dbContext;

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
        await _dbContext.SaveChangesAsync();
        return tokens;
    }

    public async Task LogoutAsync(string refreshToken, User user)
    {
        string tokenHash = _tokenService.HashRefreshToken(refreshToken);
        RefreshToken? storedToken = _dbContext.RefreshTokens
            .SingleOrDefault(token => token.TokenHash == tokenHash && token.UserId == user.Id);

        if (storedToken is not null)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<AuthTokens?> RefreshAsync(string refreshToken)
    {
        string tokenHash = _tokenService.HashRefreshToken(refreshToken);
        RefreshToken? storedToken = await _dbContext.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash);

        if (storedToken is null ||
            storedToken.RevokedAt is not null ||
            storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        storedToken.RevokedAt = DateTime.UtcNow;
        AuthTokens tokens = await IssueTokensAsync(storedToken.User);
        storedToken.ReplacedByTokenHash = _tokenService.HashRefreshToken(tokens.RefreshToken);
        await _dbContext.SaveChangesAsync();
        return tokens;
    }

    public async Task RegisterAsync(string username, string email, string password)
    {
        User user = new()
        {
            UserName = username,
            Email = email
        };
        await _userManager.CreateAsync(user, password);
    }

    private async Task<AuthTokens> IssueTokensAsync(User user)
    {
        string refreshToken = _tokenService.CreateRefreshToken();
        DateTime expiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime);

        _dbContext.RefreshTokens.Add(new RefreshToken
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
}
