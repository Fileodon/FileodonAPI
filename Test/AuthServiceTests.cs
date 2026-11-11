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
    public async Task GetMe_returns_the_requested_user()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User expectedUser = await CreateUserAsync(userManager);
        AuthService authService = CreateAuthService(scope);

        User? user = await authService.GetMeAsync(Guid.Parse(expectedUser.Id));

        Assert.NotNull(user);
        Assert.Equal(expectedUser.Id, user.Id);
        Assert.Equal(expectedUser.Email, user.Email);
    }

    [Fact]
    public async Task RequestPasswordReset_returns_failure_for_an_unknown_email_internally_but_does_not_leak_user_existence()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        AuthService authService = CreateAuthService(scope);

        IdentityResult result = await authService.RequestPasswordResetAsync("unknown@example.com");

        Assert.False(result.Succeeded);
        Assert.Empty(result.Errors);
        context.EmailSender.Verify(
            sender => sender.SendPasswordResetLinkAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task RequestPasswordReset_sends_a_reset_link_for_an_existing_email()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager);
        AuthService authService = CreateAuthService(scope);

        IdentityResult result = await authService.RequestPasswordResetAsync(user.Email!);

        Assert.True(result.Succeeded);
        context.EmailSender.Verify(
            sender => sender.SendPasswordResetLinkAsync(
                It.Is<User>(candidate => candidate.Id == user.Id)),
            Times.Once);
    }

    [Fact]
    public async Task RequestPasswordReset_returns_failure_when_reset_email_cannot_be_queued()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager);
        context.EmailSender
            .Setup(sender => sender.SendPasswordResetLinkAsync(It.Is<User>(candidate => candidate.Id == user.Id)))
            .ReturnsAsync(false);
        AuthService authService = CreateAuthService(scope);

        IdentityResult result = await authService.RequestPasswordResetAsync(user.Email!);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "EmailUnavailable");
    }

    [Fact]
    public async Task RequestEmailConfirmation_returns_failure_for_an_unknown_email()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        AuthService authService = CreateAuthService(scope);

        IdentityResult result = await authService.RequestEmailConfirmationAsync("unknown@example.com");

        Assert.False(result.Succeeded);
        context.EmailSender.Verify(
            sender => sender.SendConfirmationLinkAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task RequestEmailConfirmation_sends_a_confirmation_link_for_an_existing_unconfirmed_email()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager, false);
        AuthService authService = CreateAuthService(scope);

        IdentityResult result = await authService.RequestEmailConfirmationAsync(user.Email!);

        Assert.True(result.Succeeded);
        context.EmailSender.Verify(
            sender => sender.SendConfirmationLinkAsync(
                It.Is<User>(candidate => candidate.Id == user.Id)),
            Times.Once);
    }

    [Fact]
    public async Task RequestEmailConfirmation_returns_failure_when_confirmation_email_cannot_be_queued()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager, false);
        context.EmailSender
            .Setup(sender => sender.SendConfirmationLinkAsync(It.Is<User>(candidate => candidate.Id == user.Id)))
            .ReturnsAsync(false);
        AuthService authService = CreateAuthService(scope);

        IdentityResult result = await authService.RequestEmailConfirmationAsync(user.Email!);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "EmailUnavailable");
    }

    [Fact]
    public async Task RequestEmailConfirmation_returns_success_for_an_existing_confirmed_email()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager, true);
        AuthService authService = CreateAuthService(scope);

        IdentityResult result = await authService.RequestEmailConfirmationAsync(user.Email!);

        Assert.True(result.Succeeded);
        context.EmailSender.Verify(
            sender => sender.SendConfirmationLinkAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task ConfirmUserEmail_confirms_a_valid_token()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager);
        string token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        AuthService authService = CreateAuthService(scope);

        IdentityResult result = await authService.ConfirmUserEmailAsync(user.Id, token);

        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(error => error.Description)));
        User confirmedUser = await userManager.FindByIdAsync(user.Id)
            ?? throw new InvalidOperationException("Confirmed user was not found.");
        Assert.True(confirmedUser.EmailConfirmed);
    }

    [Fact]
    public async Task ConfirmUserEmail_fails_when_user_does_not_exist()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        AuthService authService = CreateAuthService(scope);

        IdentityResult result = await authService.ConfirmUserEmailAsync(Guid.NewGuid().ToString(), "some-token");

        Assert.False(result.Succeeded);
        Assert.Empty(result.Errors);
        context.EmailSender.Verify(
            sender => sender.SendConfirmationLinkAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task ResetPassword_changes_the_users_password()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager);
        AuthService authService = CreateAuthService(scope);
        string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

        IdentityResult result = await authService.ResetPassword(user, resetToken, NewPassword);

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
        string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

        IdentityResult result = await authService.ResetPassword(user, resetToken, newPassword);

        Assert.False(result.Succeeded);
        Assert.True(await userManager.CheckPasswordAsync(user, ExistingPassword));
        Assert.False(await userManager.CheckPasswordAsync(user, newPassword));
    }

    [Fact]
    public async Task ResetPassword_does_not_consume_the_token_if_the_password_does_not_meet_the_policy()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager);
        AuthService authService = CreateAuthService(scope);
        string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

        IdentityResult result1 = await authService.ResetPassword(user, resetToken, "short1!");
        IdentityResult result2 = await authService.ResetPassword(user, resetToken, NewPassword);

        Assert.False(result1.Succeeded);
        Assert.True(result2.Succeeded, string.Join("; ", result2.Errors.Select(error => error.Description)));
        Assert.True(await userManager.CheckPasswordAsync(user, NewPassword));
    }

    [Fact]
    public async Task ResetPassword_rejects_an_invalid_token()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = await CreateUserAsync(userManager);
        AuthService authService = CreateAuthService(scope);

        string token = "invalid-token";
        IdentityResult result = await authService.ResetPassword(user, token, NewPassword);

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
            It.Is<User>(candidate => candidate.Id == user.Id)), Times.Once);
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
            scope.ServiceProvider.GetRequiredService<ISmtpEmailSender>(),
            scope.ServiceProvider.GetRequiredService<UserManager<User>>(),
            scope.ServiceProvider.GetRequiredService<SignInManager<User>>(),
            scope.ServiceProvider.GetRequiredService<ITokenService>(),
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>());
    }

    private static DbSet<RefreshToken> GetRefreshTokens(IServiceScope scope)
    {
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().RefreshTokens;
    }

    private static async Task<User> CreateUserAsync(UserManager<User> userManager, bool emailConfirmed = true)
    {
        User user = new()
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "ada",
            Email = $"ada-{Guid.NewGuid():N}@example.com",
            EmailConfirmed = emailConfirmed
        };
        IdentityResult result = await userManager.CreateAsync(user, ExistingPassword);
        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(error => error.Description)));
        return user;
    }
}
