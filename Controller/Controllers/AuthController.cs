using Controller.DTO.Auth;
using Domain.DTO.Auth;
using Domain.Interface;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
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
                return Unauthorized("Invalid email or password.");
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
            if (user is null)
            {
                _logger.LogWarning("Logout attempt failed: User with ID {UserId} not found.", id);
                return Ok("Logged out successfully.");
            }
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

        [HttpPost("password/reset")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out Guid id))
            {
                return Unauthorized("Invalid user ID.");
            }

            var user = await _authService.GetMeAsync(id);
            if (user is null)
            {
                _logger.LogWarning("Password reset attempt failed: User with ID {UserId} not found.", id);
                return BadRequest("Failed to reset password. The token may be invalid or expired.");
            }
            var result = await _authService.ResetPassword(user, request.Token, request.NewPassword);

            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code == "InvalidToken"))
                {
                    return BadRequest("Failed to reset password. The token may be invalid or expired.");
                }
                else
                {
                    return BadRequest(result.Errors.Select(e => e.Description));
                }
            }

            return Ok("Password reset successfully.");
        }

        [HttpPost("password/request-reset")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(503)]
        public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("Email is required.");
            }

            IdentityResult result = await _authService.RequestPasswordResetAsync(request.Email);
            if (result.Errors.Any(error => error.Code == "EmailUnavailable"))
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            return Ok("If an account with that email exists, a password reset link has been sent.");
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
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

        [HttpPost("email/request-confirmation")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(503)]
        public async Task<IActionResult> RequestEmailConfirmation([FromBody] RequestEmailConfirmationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("Email is required.");
            }

            IdentityResult result = await _authService.RequestEmailConfirmationAsync(request.Email);
            if (result.Errors.Any(error => error.Code == "EmailUnavailable"))
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            return Ok("If an account with that email exists, a confirmation link has been sent.");
        }

        [HttpPost("email/confirm")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Token))
            {
                return BadRequest("UserId and Token are required.");
            }

            var result = await _authService.ConfirmUserEmailAsync(request.UserId, request.Token);

            if (!result.Succeeded)
            {
                return BadRequest("Failed to confirm email. The token may be invalid or expired.");
            }

            return Ok("Email confirmed successfully.");
        }

        // https://stackoverflow.com/questions/72188657/how-to-get-client-ip-in-net-core-6-web-api
        [HttpGet("ip")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<IActionResult> GetIp()
        {
            var ip = this.HttpContext.Connection.RemoteIpAddress;
            if (ip is null)
            {
                return BadRequest("Ip is not found");
            }

            if (ip.IsIPv4MappedToIPv6)
            {
                return Ok(ip.MapToIPv4().ToString());
            }

            return Ok(ip.ToString());
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

            User? user = await _authService.GetMeAsync(id);
            if (user is null)
            {
                _logger.LogWarning("GetMe attempt failed: User with ID {UserId} not found.", id);
                return Unauthorized("User not found.");
            }
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
