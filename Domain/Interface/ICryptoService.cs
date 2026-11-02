namespace Domain.Interface;

public interface ICryptoService
{
    byte[] Encrypt(byte[] plainText);
    byte[] Decrypt(byte[] payload);
}
