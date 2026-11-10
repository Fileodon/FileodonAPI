using System.Security.Claims;
using Controller.Controllers;
using Controller.DTO.Auth;
using Domain.DTO.Auth;
using Domain.Interface;
using Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Test;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task Login_returns_access_token_and_sets_refresh_token_cookie()
    {
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddDays(1);
        Mock<IAuthService> authService = new();
        authService
            .Setup(service => service.LoginAsync("user@example.com", "Password1!"))
            .ReturnsAsync(new AuthTokens("access-token", "refresh-token", expiresAt));
        AuthController controller = CreateController(authService, "api.example.com");

        IActionResult result = await controller.Login(new LoginRequest(
            "user@example.com",
            "Password1!"));

        OkObjectResult response = Assert.IsType<OkObjectResult>(result);
        AuthResponse authResponse = Assert.IsType<AuthResponse>(response.Value!);
        Assert.Equal("access-token", authResponse.AccessToken);
        Assert.Equal(expiresAt.ToUnixTimeSeconds(), authResponse.RefreshTokenExpiryTimestamp);
        string setCookie = controller.Response.Headers.SetCookie.ToString();
        Assert.Contains("refreshToken=refresh-token", setCookie);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/api/auth", setCookie, StringComparison.OrdinalIgnoreCase);
        authService.Verify(service => service.LoginAsync("user@example.com", "Password1!"), Times.Once);
    }

    [Fact]
    public async Task Refresh_uses_refresh_token_cookie_and_replaces_it()
    {
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddDays(1);
        Mock<IAuthService> authService = new();
        authService
            .Setup(service => service.RefreshAsync("old-refresh-token"))
            .ReturnsAsync(new AuthTokens("new-access-token", "new-refresh-token", expiresAt));
        AuthController controller = CreateController(authService);
        controller.Request.Headers.Cookie = "refreshToken=old-refresh-token";

        IActionResult result = await controller.Refresh();

        Assert.IsType<OkObjectResult>(result);
        string setCookie = controller.Response.Headers.SetCookie.ToString();
        Assert.Contains("refreshToken=new-refresh-token", setCookie);
        authService.Verify(service => service.RefreshAsync("old-refresh-token"), Times.Once);
    }

    [Fact]
    public async Task Register_rejects_a_missing_username()
    {
        Mock<IAuthService> authService = new();
        AuthController controller = CreateController(authService);

        IActionResult result = await controller.Register(new RegisterRequest(
            string.Empty,
            "user@example.com",
            "Password1!"));

        BadRequestObjectResult response = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Username, email, and password are required.", response.Value);
        authService.Verify(
            service => service.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Logout_uses_refresh_token_cookie()
    {
        Mock<IAuthService> authService = new();
        AuthController controller = CreateController(authService);
        Guid userId = Guid.NewGuid();
        User user = new() { Id = userId.ToString() };
        authService.Setup(service => service.GetMeAsync(userId)).ReturnsAsync(user);
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(
                [new Claim("id", userId.ToString())],
                "test"));
        controller.Request.Headers.Cookie = "refreshToken=logout-refresh-token";

        IActionResult result = await controller.Logout();

        Assert.IsType<OkObjectResult>(result);
        authService.Verify(
            service => service.LogoutAsync("logout-refresh-token", user),
            Times.Once);
    }

    [Fact]
    public async Task Login_rejects_missing_credentials_without_calling_service()
    {
        Mock<IAuthService> authService = new();
        AuthController controller = CreateController(authService);

        IActionResult result = await controller.Login(new LoginRequest(string.Empty, "Password1!"));

        BadRequestObjectResult response = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Email and password are required.", response.Value);
        authService.Verify(service => service.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Login_returns_unauthorized_when_credentials_are_invalid()
    {
        Mock<IAuthService> authService = new();
        authService.Setup(service => service.LoginAsync("user@example.com", "wrong")).ReturnsAsync((AuthTokens?)null);
        AuthController controller = CreateController(authService);

        IActionResult result = await controller.Login(new LoginRequest("user@example.com", "wrong"));

        UnauthorizedObjectResult response = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid email or password.", response.Value);
    }

    [Fact]
    public async Task Refresh_rejects_missing_cookie_without_calling_service()
    {
        Mock<IAuthService> authService = new();
        AuthController controller = CreateController(authService);

        IActionResult result = await controller.Refresh();

        BadRequestObjectResult response = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Refresh token is required.", response.Value);
        authService.Verify(service => service.RefreshAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Refresh_returns_unauthorized_when_token_is_invalid()
    {
        Mock<IAuthService> authService = new();
        authService.Setup(service => service.RefreshAsync("invalid")).ReturnsAsync((AuthTokens?)null);
        AuthController controller = CreateController(authService);
        controller.Request.Headers.Cookie = "refreshToken=invalid";

        IActionResult result = await controller.Refresh();

        UnauthorizedObjectResult response = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid or expired refresh token.", response.Value);
    }

    [Fact]
    public async Task Logout_rejects_an_invalid_user_id()
    {
        Mock<IAuthService> authService = new();
        AuthController controller = CreateController(authService);
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim("id", "not-a-guid")], "test"));

        IActionResult result = await controller.Logout();

        UnauthorizedObjectResult response = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid user ID.", response.Value);
        authService.Verify(service => service.GetMeAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Logout_rejects_a_missing_refresh_token()
    {
        Mock<IAuthService> authService = new();
        AuthController controller = CreateController(authService);
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim("id", Guid.NewGuid().ToString())], "test"));

        IActionResult result = await controller.Logout();

        BadRequestObjectResult response = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Refresh token is missing.", response.Value);
        authService.Verify(service => service.GetMeAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ResetPassword_returns_unauthorized_for_an_invalid_user_id()
    {
        Mock<IAuthService> authService = new();
        AuthController controller = CreateController(authService);

        IActionResult result = await controller.ResetPassword(new ResetPasswordRequest("invalid", "token", "Password1!"));

        UnauthorizedObjectResult response = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid user ID.", response.Value);
        authService.Verify(service => service.GetMeAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ResetPassword_does_not_leak_information_when_service_fails()
    {
        Guid userId = Guid.NewGuid();
        User user = new() { Id = userId.ToString() };
        Mock<IAuthService> authService = new();
        authService.Setup(service => service.GetMeAsync(userId)).ReturnsAsync(user);
        authService
            .Setup(service => service.ResetPassword(user, "token", "Password1!"))
            .ReturnsAsync(IdentityResult.Failed());
        AuthController controller = CreateControllerWithUser(authService, userId);

        IActionResult result = await controller.ResetPassword(new ResetPasswordRequest(userId.ToString(), "token", "Password1!"));

        BadRequestObjectResult response = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to reset password. The token may be invalid or expired.", response.Value);
    }

    [Fact]
    public async Task ResetPassword_returns_success_when_service_succeeds()
    {
        Guid userId = Guid.NewGuid();
        User user = new() { Id = userId.ToString() };
        Mock<IAuthService> authService = new();
        authService.Setup(service => service.GetMeAsync(userId)).ReturnsAsync(user);
        authService
            .Setup(service => service.ResetPassword(user, "token", "Password1!"))
            .ReturnsAsync(IdentityResult.Success);
        AuthController controller = CreateControllerWithUser(authService, userId);

        IActionResult result = await controller.ResetPassword(new ResetPasswordRequest(userId.ToString(), "token", "Password1!"));

        OkObjectResult response = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Password reset successfully.", response.Value);
    }

    [Fact]
    public async Task RequestPasswordReset_rejects_missing_email()
    {
        Mock<IAuthService> authService = new();
        AuthController controller = CreateController(authService);

        IActionResult result = await controller.RequestPasswordReset(new RequestPasswordResetRequest(" "));

        BadRequestObjectResult response = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Email is required.", response.Value);
        authService.Verify(service => service.RequestPasswordResetAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RequestPasswordReset_does_not_leak_information_when_user_does_not_exist()
    {
        Mock<IAuthService> authService = new();
        authService
            .Setup(service => service.RequestPasswordResetAsync("missing@example.com"))
            .ReturnsAsync(IdentityResult.Failed());
        AuthController controller = CreateController(authService);

        IActionResult result = await controller.RequestPasswordReset(new RequestPasswordResetRequest("missing@example.com"));

        OkObjectResult response = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("If an account with that email exists, a password reset link has been sent.", response.Value);
    }

    [Fact]
    public async Task RequestPasswordReset_returns_success_when_service_succeeds()
    {
        Mock<IAuthService> authService = new();
        authService
            .Setup(service => service.RequestPasswordResetAsync("user@example.com"))
            .ReturnsAsync(IdentityResult.Success);
        AuthController controller = CreateController(authService);

        IActionResult result = await controller.RequestPasswordReset(new RequestPasswordResetRequest("user@example.com"));

        OkObjectResult response = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("If an account with that email exists, a password reset link has been sent.", response.Value);
    }

    [Fact]
    public async Task Register_returns_bad_request_when_service_fails()
    {
        Mock<IAuthService> authService = new();
        authService
            .Setup(service => service.RegisterAsync("ada", "ada@example.com", "Password1!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Duplicate email." }));
        AuthController controller = CreateController(authService);

        IActionResult result = await controller.Register(new RegisterRequest("ada", "ada@example.com", "Password1!"));

        BadRequestObjectResult response = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Duplicate email.", (IEnumerable<string>)response.Value!);
    }

    [Fact]
    public async Task Register_returns_created_when_service_succeeds()
    {
        Mock<IAuthService> authService = new();
        authService
            .Setup(service => service.RegisterAsync("ada", "ada@example.com", "Password1!"))
            .ReturnsAsync(IdentityResult.Success);
        AuthController controller = CreateController(authService);

        IActionResult result = await controller.Register(new RegisterRequest("ada", "ada@example.com", "Password1!"));

        CreatedAtActionResult response = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(AuthController.Register), response.ActionName);
        Assert.Equal("User registered successfully.", response.Value);
    }

    [Fact]
    public async Task RequestEmailConfirmation_rejects_missing_email()
    {
        Mock<IAuthService> authService = new();
        AuthController controller = CreateController(authService);

        IActionResult result = await controller.RequestEmailConfirmation(new RequestEmailConfirmationRequest(string.Empty));

        BadRequestObjectResult response = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Email is required.", response.Value);
        authService.Verify(service => service.RequestEmailConfirmationAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RequestEmailConfirmation_returns_success_when_service_succeeds()
    {
        Mock<IAuthService> authService = new();
        authService
            .Setup(service => service.RequestEmailConfirmationAsync("user@example.com"))
            .ReturnsAsync(IdentityResult.Success);
        AuthController controller = CreateController(authService);

        IActionResult result = await controller.RequestEmailConfirmation(new RequestEmailConfirmationRequest("user@example.com"));

        OkObjectResult response = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("If an account with that email exists, a confirmation link has been sent.", response.Value);
    }

    [Fact]
    public async Task ConfirmEmail_rejects_missing_values()
    {
        Mock<IAuthService> authService = new();
        AuthController controller = CreateController(authService);

        IActionResult result = await controller.ConfirmEmail(new ConfirmEmailRequest(string.Empty, "token"));

        BadRequestObjectResult response = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("UserId and Token are required.", response.Value);
        authService.Verify(service => service.ConfirmUserEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmEmail_does_not_leak_information_when_user_does_not_exist()
    {
        Mock<IAuthService> authService = new();
        authService
            .Setup(service => service.ConfirmUserEmailAsync("user-id", "token"))
            .ReturnsAsync(IdentityResult.Failed());
        AuthController controller = CreateController(authService);

        IActionResult result = await controller.ConfirmEmail(new ConfirmEmailRequest("user-id", "token"));

        BadRequestObjectResult response = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to confirm email. The token may be invalid or expired.", response.Value);
    }

    [Fact]
    public async Task ConfirmEmail_returns_success_when_service_succeeds()
    {
        Mock<IAuthService> authService = new();
        authService
            .Setup(service => service.ConfirmUserEmailAsync("user-id", "token"))
            .ReturnsAsync(IdentityResult.Success);
        AuthController controller = CreateController(authService);

        IActionResult result = await controller.ConfirmEmail(new ConfirmEmailRequest("user-id", "token"));

        OkObjectResult response = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Email confirmed successfully.", response.Value);
    }

    [Fact]
    public async Task GetMe_returns_unauthorized_for_an_invalid_user_id()
    {
        Mock<IAuthService> authService = new();
        AuthController controller = CreateController(authService);

        IActionResult result = await controller.GetMe();

        UnauthorizedObjectResult response = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid user ID.", response.Value);
        authService.Verify(service => service.GetMeAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetMe_returns_basic_user_information()
    {
        Guid userId = Guid.NewGuid();
        User user = new()
        {
            Id = userId.ToString(),
            Email = "ada@example.com",
            UserName = "ada",
            EmailConfirmed = true
        };
        Mock<IAuthService> authService = new();
        authService.Setup(service => service.GetMeAsync(userId)).ReturnsAsync(user);
        AuthController controller = CreateControllerWithUser(authService, userId);

        IActionResult result = await controller.GetMe();

        OkObjectResult response = Assert.IsType<OkObjectResult>(result);
        BasicUserInformationResponse userResponse = Assert.IsType<BasicUserInformationResponse>(response.Value);
        Assert.Equal(userId, userResponse.Id);
        Assert.Equal("ada@example.com", userResponse.Email);
        Assert.Equal("ada", userResponse.Username);
        Assert.True(userResponse.IsEmailConfirmed);
    }

    private static AuthController CreateController(
        Mock<IAuthService> authService,
        string host = "localhost")
    {
        Mock<ILogger<AuthController>> logger = new();
        AuthController controller = new(authService.Object, logger.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.Request.Host = new HostString(host);
        return controller;
    }

    private static AuthController CreateControllerWithUser(Mock<IAuthService> authService, Guid userId)
    {
        AuthController controller = CreateController(authService);
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim("id", userId.ToString())], "test"));
        return controller;
    }
}
