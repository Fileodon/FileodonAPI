using Controller.DTO.Auth;
using Domain.Interface;
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
                return BadRequest("Username and password are required.");
            }

            AuthTokens? tokens = await _authService.LoginAsync(request.Email, request.Password);

            if (tokens == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            return Ok(new AuthResponse(
                tokens.AccessToken,
                tokens.RefreshToken,
                tokens.RefreshTokenExpiresAt.ToUnixTimeSeconds()));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest("Refresh token is required.");
            }

            AuthTokens? tokens = await _authService.RefreshAsync(request.RefreshToken);

            if (tokens == null)
            {
                return Unauthorized("Invalid or expired refresh token.");
            }

            return Ok(new AuthResponse(
                tokens.AccessToken,
                tokens.RefreshToken,
                tokens.RefreshTokenExpiresAt.ToUnixTimeSeconds()));
        }

        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required.");
            }

            await _authService.RegisterAsync(request.Username, request.Email, request.Password);
            return Ok("User registered successfully.");
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            string? userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid id))
            {
                return Unauthorized("Invalid user ID.");
            }

            var user = await _authService.GetMeAsync(id);
            return Ok(new { user.UserName, user.Email });
        }
    }
}
