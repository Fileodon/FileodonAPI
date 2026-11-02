using System.Security.Claims;
using Controller.Controllers;
using Controller.DTO.Auth;
using Domain.DTO.Auth;
using Domain.Interface;
using Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

    private static AuthController CreateController(
        Mock<IAuthService> authService,
        string host = "localhost")
    {
        AuthController controller = new(authService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.Request.Host = new HostString(host);
        return controller;
    }
}
