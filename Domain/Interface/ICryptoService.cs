using Domain.DTO;

namespace Domain.Interface;

public interface ICryptoService
{
    byte[] Encrypt(byte[] plainText);
    byte[] Decrypt(byte[] payload);

    RSAKeyPairResult CreateRSAKeyPair(int keySize = 4096);
}
