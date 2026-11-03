namespace Domain.DTO;

public record class RSAKeyPairResult(
    byte[] PublicKey,
    byte[] PrivateKey
);
