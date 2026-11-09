namespace Infrastructure.Models.DTO;

public record RSAKeyPairResult(
    byte[] PublicKey,
    byte[] PrivateKey
);
