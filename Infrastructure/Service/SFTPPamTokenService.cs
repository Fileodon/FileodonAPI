using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Infrastructure.Interface;
using Infrastructure.Models;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Service;

public sealed class SFTPPamTokenService(ICryptoService cryptoService) : ISFTPPamTokenService
{
    public string CreateToken(SFTPConnection connection, Guid jobId)
    {
        ArgumentException.ThrowIfNullOrEmpty(connection.PrivateKey);

        byte[] encryptedKey = Convert.FromBase64String(connection.PrivateKey);
        byte[] privateKey = cryptoService.Decrypt(encryptedKey);
        using RSA rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(privateKey, out _);

        DateTime expiresAt = DateTime.UtcNow.AddMinutes(2);
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, "fileodon"),
            new("job_id", jobId.ToString("D")),
            new("destination_connection_id", connection.Id.ToString())
        ];
        SigningCredentials credentials = new(
            new RsaSecurityKey(rsa),
            SecurityAlgorithms.RsaSha256);
        JwtSecurityToken token = new(
            issuer: "Fileodon Master Server",
            audience: connection.Host,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
