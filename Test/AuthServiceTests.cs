using Domain.DTO.Auth;
using Domain.Interface;
using Domain.Service;
using Infrastructure;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Test;

public sealed class AuthServiceTests
{
    private const string ExistingPassword = "Password1!";
    private const string NewPassword = "NewPassword1!";

    [Fact]
    public async Task Login_returns_access_and_refresh_tokens_and_persists_refresh_token()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager);
        AuthService authService = CreateAuthService(scope);

        AuthTokens? tokens = await authService.LoginAsync(user.Email!, ExistingPassword);

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
        AuthTokens originalTokens = (await authService.LoginAsync(user.Email!, ExistingPassword))!;

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
        AuthTokens originalTokens = (await authService.LoginAsync(user.Email!, ExistingPassword))!;

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
        AuthTokens tokens = (await authService.LoginAsync(user.Email!, ExistingPassword))!;

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

        IdentityResult result = await authService.ResetPassword(user, ExistingPassword, NewPassword);

        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(error => error.Description)));
        Assert.True(await userManager.CheckPasswordAsync(user, NewPassword));
        Assert.False(await userManager.CheckPasswordAsync(user, ExistingPassword));
    }

    [Theory]
    [InlineData("short1!")]
    [InlineData("newpassword!")]
    [InlineData("NewPassword!")]
    [InlineData("NewPassword1")]
    public async Task ResetPassword_rejects_passwords_that_do_not_meet_the_policy(string newPassword)
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager);
        AuthService authService = CreateAuthService(scope);

        IdentityResult result = await authService.ResetPassword(user, ExistingPassword, newPassword);

        Assert.False(result.Succeeded);
        Assert.True(await userManager.CheckPasswordAsync(user, ExistingPassword));
        Assert.False(await userManager.CheckPasswordAsync(user, newPassword));
    }

    [Fact]
    public async Task ResetPassword_rejects_an_incorrect_old_password()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager);
        AuthService authService = CreateAuthService(scope);

        IdentityResult result = await authService.ResetPassword(user, "IncorrectPassword1!", NewPassword);

        Assert.False(result.Succeeded);
        Assert.True(await userManager.CheckPasswordAsync(user, ExistingPassword));
        Assert.False(await userManager.CheckPasswordAsync(user, NewPassword));
    }

    [Fact]
    public async Task Register_creates_a_user_with_a_password_that_meets_the_policy()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        AuthService authService = CreateAuthService(scope);

        IdentityResult result = await authService.RegisterAsync(
            "grace",
            "grace@example.com",
            NewPassword);

        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(error => error.Description)));
        User user = await userManager.FindByEmailAsync("grace@example.com")
            ?? throw new InvalidOperationException("Registered user was not found.");
        Assert.True(await userManager.CheckPasswordAsync(user, NewPassword));
        context.EmailSender.Verify(sender => sender.SendConfirmationLinkAsync(
            It.Is<User>(candidate => candidate.Id == user.Id),
            "grace@example.com",
            It.Is<string>(link => link.Contains($"userId={user.Id}"))), Times.Once);
    }

    [Theory]
    [InlineData("short1!")]
    [InlineData("newpassword!")]
    [InlineData("NewPassword!")]
    [InlineData("NewPassword1")]
    public async Task Register_rejects_passwords_that_do_not_meet_the_policy(string password)
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        AuthService authService = CreateAuthService(scope);

        IdentityResult result = await authService.RegisterAsync(
            "grace",
            "grace@example.com",
            password);

        Assert.False(result.Succeeded);
        Assert.Null(await userManager.FindByEmailAsync("grace@example.com"));
    }

    [Fact]
    public async Task Register_rejects_an_email_that_is_already_registered()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        AuthService authService = CreateAuthService(scope);
        const string email = "grace@example.com";

        IdentityResult firstResult = await authService.RegisterAsync("grace", email, NewPassword);
        IdentityResult secondResult = await authService.RegisterAsync("another-grace", email, NewPassword);

        Assert.True(firstResult.Succeeded, string.Join("; ", firstResult.Errors.Select(error => error.Description)));
        Assert.False(secondResult.Succeeded);
        Assert.Contains(secondResult.Errors, error => error.Code == "DuplicateEmail");
    }

    private static AuthService CreateAuthService(IServiceScope scope)
    {
        return new AuthService(
            scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>(),
            scope.ServiceProvider.GetRequiredService<IEmailSender<User>>(),
            scope.ServiceProvider.GetRequiredService<UserManager<User>>(),
            scope.ServiceProvider.GetRequiredService<SignInManager<User>>(),
            scope.ServiceProvider.GetRequiredService<ITokenService>(),
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>());
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
        IdentityResult result = await userManager.CreateAsync(user, ExistingPassword);
        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(error => error.Description)));
        return user;
    }
}
