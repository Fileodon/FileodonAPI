using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Infrastructure.Interface;
using Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Service;

public sealed class SFTPIntermediaryTokenService(IConfiguration configuration) : ISFTPIntermediaryTokenService
{
    public string CreateToken(SFTPConnection destinationConnection, Guid jobId)
    {
        string privateKeyPath = configuration["SFTPPamJwt:PrivateKeyPath"]
            ?? throw new InvalidOperationException(
                "SFTPPamJwt:PrivateKeyPath is not set in configuration.");
        using ECDsa ecdsa = ECDsa.Create();
        try
        {
            ecdsa.ImportPkcs8PrivateKey(File.ReadAllBytes(privateKeyPath), out _);
        }
        catch (Exception ex) when (ex is IOException or CryptographicException)
        {
            throw new InvalidOperationException(
                $"Unable to load the SFTP PAM JWT private key from '{privateKeyPath}'.",
                ex);
        }

        DateTime expiresAt = DateTime.UtcNow.AddMinutes(30);
        ECDsaSecurityKey signingKey = new(ecdsa)
        {
            KeyId = configuration["SFTPPamJwt:KeyId"] ?? "fileodon-es256-v1",
            CryptoProviderFactory = new CryptoProviderFactory
            {
                CacheSignatureProviders = false
            }
        };
        SigningCredentials credentials = new(
            signingKey,
            SecurityAlgorithms.EcdsaSha256);
        JwtSecurityToken token = new(
            issuer: "Fileodon Master Server",
            audience: destinationConnection.Host,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
