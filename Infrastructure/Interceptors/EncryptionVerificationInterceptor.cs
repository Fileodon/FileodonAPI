using System.Security.Cryptography;
using Infrastructure.Interface;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Interceptors;

public class EncryptionVerificationInterceptor : SaveChangesInterceptor
{
    private readonly ICryptoService _cryptoService;

    public EncryptionVerificationInterceptor(ICryptoService cryptoService)
    {
        _cryptoService = cryptoService;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        VerifyEncryption(eventData);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        VerifyEncryption(eventData);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void VerifyEncryption(DbContextEventData eventData)
    {
        var context = eventData.Context;
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Modified || entry.State == EntityState.Added)
            {
                if (entry.Entity is SFTPConnection)
                {
                    var currentPublicKey = entry.CurrentValues.GetValue<string>("PublicKey");
                    if (!string.IsNullOrEmpty(currentPublicKey))
                    {
                        // Verify that the public key is valid
                        try
                        {
                            var publicKeyBytes = Convert.FromBase64String(currentPublicKey);
                            _cryptoService.Decrypt(publicKeyBytes);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException("The provided public key is invalid. Did you forget to encrypt it?", ex);
                        }
                    }
                    var currentPrivateKey = entry.CurrentValues.GetValue<string>("PrivateKey");
                    if (!string.IsNullOrEmpty(currentPrivateKey))
                    {
                        // Verify that the private key is valid
                        try
                        {
                            var privateKeyBytes = Convert.FromBase64String(currentPrivateKey);
                            _cryptoService.Decrypt(privateKeyBytes);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException("The provided private key is invalid. Did you forget to encrypt it?", ex);
                        }
                    }
                    // Both must change
                    if (!string.IsNullOrEmpty(currentPublicKey) && !string.IsNullOrEmpty(currentPrivateKey))
                    {
                        // Verify that the public and private keys match
                        try
                        {
                            var publicKeyBytes = Convert.FromBase64String(currentPublicKey);
                            var privateKeyBytes = Convert.FromBase64String(currentPrivateKey);
                            var decryptedPublicKey = _cryptoService.Decrypt(publicKeyBytes);
                            var decryptedPrivateKey = _cryptoService.Decrypt(privateKeyBytes);
                            using RSA rsa = RSA.Create();
                            rsa.ImportRSAPrivateKey(decryptedPrivateKey, out _);

                            if (!rsa.ExportRSAPublicKey().SequenceEqual(decryptedPublicKey))
                            {
                                throw new InvalidOperationException("The provided public and private keys do not match.");
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException("The provided public and private keys do not match.", ex);
                        }
                    }
                }
            }
        }
    }
}
