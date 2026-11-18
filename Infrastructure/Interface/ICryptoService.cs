using System.Security.Cryptography;
using Infrastructure.Models.DTO;

namespace Infrastructure.Interface;

public interface ICryptoService
{
    byte[] Encrypt(byte[] plainText);
    byte[] Decrypt(byte[] payload);

    RSAKeyPairResult CreateRSAKeyPair(int keySize = 4096);
    string ConvertPublicKeyToPem(byte[] publicKey);
    string ConvertPrivateKeyToPem(byte[] privateKey);
    string ConvertPublicKeyToOpenSsh(byte[] publicKey, string? comment = null);
}
