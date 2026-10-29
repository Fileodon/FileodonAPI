using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Interface;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Domain.Service;

public class TokenService(IConfiguration configuration, UserManager<User> userManager) : ITokenService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly UserManager<User> _userManager = userManager;

    public async Task<string> CreateTokenAsync(User user)
    {
        List<Claim> claims =
        [
            new Claim("id", user.Id.ToString()),
            new Claim("name", user.UserName ?? string.Empty),
            new Claim("email", user.Email ?? string.Empty),
        ];

        foreach (string role in await _userManager.GetRolesAsync(user))
        {
            claims.Add(new Claim("role", role));
        }

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_configuration["JWT_KEY"] ?? throw new InvalidOperationException("JWT key is not set in configuration.")));
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public string HashRefreshToken(string refreshToken)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
    }

    public static Task<string> GenerateSecret()
    {
        byte[] secretBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(secretBytes);
        }
        return Task.FromResult(Convert.ToBase64String(secretBytes));
    }
}
