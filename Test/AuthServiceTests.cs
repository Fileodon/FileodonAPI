using Domain.DTO.Auth;
using Domain.Interface;
using Domain.Service;
using Infrastructure;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Test;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task Login_returns_access_and_refresh_tokens_and_persists_refresh_token()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager);
        AuthService authService = CreateAuthService(scope);

        AuthTokens? tokens = await authService.LoginAsync(user.Email!, "password");

        Assert.NotNull(tokens);
        Assert.NotEqual(tokens.AccessToken, tokens.RefreshToken);
        Assert.True(tokens.RefreshTokenExpiresAt > DateTimeOffset.UtcNow);
        RefreshToken storedToken = await GetRefreshTokens(scope).SingleAsync();
        Assert.Equal(user.Id, storedToken.UserId);
        Assert.Equal(
            scope.ServiceProvider.GetRequiredService<ITokenService>().HashRefreshToken(tokens.RefreshToken),
            storedToken.TokenHash);
    }

    [Fact]
    public async Task Refresh_rotates_the_token_and_revokes_the_previous_token()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager);
        AuthService authService = CreateAuthService(scope);
        AuthTokens originalTokens = (await authService.LoginAsync(user.Email!, "password"))!;

        AuthTokens? refreshedTokens = await authService.RefreshAsync(originalTokens.RefreshToken);

        Assert.NotNull(refreshedTokens);
        Assert.NotEqual(originalTokens.RefreshToken, refreshedTokens.RefreshToken);
        List<RefreshToken> storedTokens = await GetRefreshTokens(scope).OrderBy(token => token.Id).ToListAsync();
        Assert.Equal(2, storedTokens.Count);
        Assert.NotNull(storedTokens[0].RevokedAt);
        Assert.Equal(
            storedTokens[1].TokenHash,
            storedTokens[0].ReplacedByTokenHash);
    }

    [Fact]
    public async Task Refresh_rejects_a_token_that_has_already_been_rotated()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager);
        AuthService authService = CreateAuthService(scope);
        AuthTokens originalTokens = (await authService.LoginAsync(user.Email!, "password"))!;

        Assert.NotNull(await authService.RefreshAsync(originalTokens.RefreshToken));

        AuthTokens? reusedTokens = await authService.RefreshAsync(originalTokens.RefreshToken);

        Assert.Null(reusedTokens);
    }

    [Fact]
    public async Task Refresh_rejects_an_unknown_token()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        AuthService authService = CreateAuthService(scope);

        AuthTokens? tokens = await authService.RefreshAsync("unknown-refresh-token");

        Assert.Null(tokens);
    }

    [Fact]
    public async Task Refresh_rejects_an_expired_token()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager);
        ITokenService tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        string refreshToken = tokenService.CreateRefreshToken();
        GetRefreshTokens(scope).Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenService.HashRefreshToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        });
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().SaveChangesAsync();
        AuthService authService = CreateAuthService(scope);

        AuthTokens? tokens = await authService.RefreshAsync(refreshToken);

        Assert.Null(tokens);
    }

    [Fact]
    public async Task Logout_revokes_the_refresh_token()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager);
        AuthService authService = CreateAuthService(scope);
        AuthTokens tokens = (await authService.LoginAsync(user.Email!, "password"))!;

        await authService.LogoutAsync(tokens.RefreshToken, user);

        RefreshToken storedToken = await GetRefreshTokens(scope).SingleAsync();
        Assert.NotNull(storedToken.RevokedAt);
    }

    [Fact]
    public async Task ResetPassword_changes_the_users_password()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager);
        AuthService authService = CreateAuthService(scope);

        IdentityResult result = await authService.ResetPassword(user, "password", "new-password");

        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(error => error.Description)));
        Assert.True(await userManager.CheckPasswordAsync(user, "new-password"));
        Assert.False(await userManager.CheckPasswordAsync(user, "password"));
    }

    [Fact]
    public async Task ResetPassword_rejects_an_incorrect_old_password()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager);
        AuthService authService = CreateAuthService(scope);

        IdentityResult result = await authService.ResetPassword(user, "incorrect-password", "new-password");

        Assert.False(result.Succeeded);
        Assert.True(await userManager.CheckPasswordAsync(user, "password"));
        Assert.False(await userManager.CheckPasswordAsync(user, "new-password"));
    }

    private static AuthService CreateAuthService(IServiceScope scope)
    {
        return new AuthService(
            scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>(),
            scope.ServiceProvider.GetRequiredService<UserManager<User>>(),
            scope.ServiceProvider.GetRequiredService<SignInManager<User>>(),
            scope.ServiceProvider.GetRequiredService<ITokenService>(),
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    private static DbSet<RefreshToken> GetRefreshTokens(IServiceScope scope)
    {
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().RefreshTokens;
    }

    private static async Task<User> CreateUserAsync(UserManager<User> userManager)
    {
        User user = new()
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "ada",
            Email = $"ada-{Guid.NewGuid():N}@example.com",
            EmailConfirmed = true
        };
        IdentityResult result = await userManager.CreateAsync(user, "password");
        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(error => error.Description)));
        return user;
    }
}
