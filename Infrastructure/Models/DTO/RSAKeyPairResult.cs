namespace Infrastructure.Models.DTO;

public record class RSAKeyPairResult(
    byte[] PublicKey,
    byte[] PrivateKey
);
