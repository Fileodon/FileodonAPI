using Controller.DTO.Auth;
using Domain.DTO.Auth;
using Domain.Interface;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email and password are required.");
            }

            AuthTokens? tokens = await _authService.LoginAsync(request.Email, request.Password);

            if (tokens == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            SetRefreshTokenCookie(tokens.RefreshToken, tokens.RefreshTokenExpiresAt);

            return Ok(new AuthResponse(
                tokens.AccessToken,
                tokens.RefreshTokenExpiresAt.ToUnixTimeSeconds()));
        }

        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Logout()
        {
            string? userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid id))
            {
                return Unauthorized("Invalid user ID.");
            }

            string? refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest("Refresh token is missing.");
            }

            var user = await _authService.GetMeAsync(id);
            await _authService.LogoutAsync(refreshToken, user);
            return Ok("Logged out successfully.");
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Refresh()
        {
            string? refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest("Refresh token is required.");
            }

            AuthTokens? tokens = await _authService.RefreshAsync(refreshToken);

            if (tokens == null)
            {
                return Unauthorized("Invalid or expired refresh token.");
            }

            SetRefreshTokenCookie(tokens.RefreshToken, tokens.RefreshTokenExpiresAt);

            return Ok(new AuthResponse(
                tokens.AccessToken,
                tokens.RefreshTokenExpiresAt.ToUnixTimeSeconds()));
        }

        [HttpPost("reset-password")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            string? userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid id))
            {
                return Unauthorized("Invalid user ID.");
            }

            var user = await _authService.GetMeAsync(id);
            var result = await _authService.ResetPassword(user, request.OldPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return Ok("Password reset successfully.");
        }


        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username, email, and password are required.");
            }

            var result = await _authService.RegisterAsync(request.Username, request.Email, request.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return CreatedAtAction(nameof(Register), new { email = request.Email }, "User registered successfully.");
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(BasicUserInformationResponse), 200)]
        public async Task<IActionResult> GetMe()
        {
            string? userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid id))
            {
                return Unauthorized("Invalid user ID.");
            }

            User user = await _authService.GetMeAsync(id);
            return Ok(await user.ToBasicUserInformationResponse());
        }

        private void SetRefreshTokenCookie(string refreshToken, DateTimeOffset expiresAt)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Stopper adgang til JavaScript
                Expires = expiresAt.UtcDateTime,
                Secure = true, // Sørger for, at cookien kun sendes over HTTPS
                SameSite = SameSiteMode.Strict, // Stopper cross-site request forgery (CSRF)
                Path = "/api/auth", // Sørger for, at cookien kun sendes til /api/auth endpoints
                Domain = Request.Host.Host // Sørger for, at cookien kun sendes til den aktuelle domæne
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
