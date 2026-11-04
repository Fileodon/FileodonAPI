using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using Domain.DTO;
using Domain.Service;
using Infrastructure.Models.DTO;
using Infrastructure.Service;

namespace Test;

public sealed class CryptoServiceTests
{
    [Fact]
    public void CreateKey_writes_a_256_bit_key_and_returns_it()
    {
        using TemporaryDirectory directory = new();
        string keyPath = Path.Combine(directory.Path, "encryption_key.bin");

        byte[] key = CryptoService.CreateKey(keyPath);

        Assert.Equal(32, key.Length);
        Assert.Equal(key, File.ReadAllBytes(keyPath));
    }

    [Fact]
    public void CreateKey_does_not_overwrite_an_existing_key()
    {
        using TemporaryDirectory directory = new();
        string keyPath = Path.Combine(directory.Path, "encryption_key.bin");
        byte[] originalKey = RandomNumberGenerator.GetBytes(32);
        File.WriteAllBytes(keyPath, originalKey);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => CryptoService.CreateKey(keyPath));

        Assert.Contains("already exists", exception.Message);
        Assert.Equal(originalKey, File.ReadAllBytes(keyPath));
    }

    [Fact]
    public void CreateKey_sets_owner_only_unix_permissions()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        using TemporaryDirectory directory = new();
        string keyPath = Path.Combine(directory.Path, "encryption_key.bin");

        CryptoService.CreateKey(keyPath);

        Assert.Equal(
            UnixFileMode.UserRead | UnixFileMode.UserWrite,
            File.GetUnixFileMode(keyPath));
    }

    [Fact]
    public void CreateKey_sets_a_restricted_windows_acl()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        using TemporaryDirectory directory = new();
        string keyPath = Path.Combine(directory.Path, "encryption_key.bin");

        CryptoService.CreateKey(keyPath);

        AssertWindowsAcl(keyPath);
    }

    [SupportedOSPlatform("windows")]
    private static void AssertWindowsAcl(string keyPath)
    {
        FileSecurity security = new FileInfo(keyPath).GetAccessControl();
        Assert.True(security.AreAccessRulesProtected);

        string currentUser = WindowsIdentity.GetCurrent().User!.Value;
        Assert.Contains(
            security.GetAccessRules(true, true, typeof(SecurityIdentifier))
                .Cast<FileSystemAccessRule>(),
            rule => rule.IdentityReference.Value == currentUser &&
                rule.AccessControlType == AccessControlType.Allow &&
                rule.FileSystemRights.HasFlag(FileSystemRights.FullControl));

        SecurityIdentifier administrators = new(
            WellKnownSidType.BuiltinAdministratorsSid,
            null);
        Assert.Contains(
            security.GetAccessRules(true, true, typeof(SecurityIdentifier))
                .Cast<FileSystemAccessRule>(),
            rule => rule.IdentityReference == administrators &&
                rule.AccessControlType == AccessControlType.Allow &&
                rule.FileSystemRights.HasFlag(FileSystemRights.Read));
    }

    [Fact]
    public void Encrypt_and_decrypt_round_trip_the_plaintext()
    {
        using TemporaryDirectory directory = new();
        string keyPath = CreateKey(directory);
        using CryptoService cryptoService = new(keyPath);
        byte[] plaintext = "Sensitive file contents"u8.ToArray();

        byte[] payload = cryptoService.Encrypt(plaintext);
        byte[] decrypted = cryptoService.Decrypt(payload);

        Assert.Equal(plaintext, decrypted);
        Assert.Equal(12 + plaintext.Length + 16, payload.Length);
    }

    [Fact]
    public void Encrypt_generates_a_different_payload_for_the_same_plaintext()
    {
        using TemporaryDirectory directory = new();
        string keyPath = CreateKey(directory);
        using CryptoService cryptoService = new(keyPath);
        byte[] plaintext = "Sensitive file contents"u8.ToArray();

        byte[] firstPayload = cryptoService.Encrypt(plaintext);
        byte[] secondPayload = cryptoService.Encrypt(plaintext);

        Assert.NotEqual(firstPayload, secondPayload);
        Assert.NotEqual(
            firstPayload[..12],
            secondPayload[..12]);
    }

    [Fact]
    public void CreateRSAKeyPair_returns_a_valid_4096_bit_key_pair()
    {
        using TemporaryDirectory directory = new();
        using CryptoService cryptoService = new(CreateKey(directory));

        RSAKeyPairResult keyPair = cryptoService.CreateRSAKeyPair();

        using RSA privateKey = RSA.Create();
        using RSA publicKey = RSA.Create();
        privateKey.ImportRSAPrivateKey(keyPair.PrivateKey, out int privateBytesRead);
        publicKey.ImportRSAPublicKey(keyPair.PublicKey, out int publicBytesRead);

        Assert.Equal(keyPair.PrivateKey.Length, privateBytesRead);
        Assert.Equal(keyPair.PublicKey.Length, publicBytesRead);
        Assert.Equal(4096, privateKey.KeySize);
        Assert.Equal(4096, publicKey.KeySize);
    }

    [Fact]
    public void CreateRSAKeyPair_rejects_a_key_size_below_2048_bits()
    {
        using TemporaryDirectory directory = new();
        using CryptoService cryptoService = new(CreateKey(directory));

        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => cryptoService.CreateRSAKeyPair(1024));

        Assert.Contains("must be greater than", exception.Message);
    }

    [Fact]
    public void CreateRSAKeyPair_rejects_a_key_size_above_16384_bits()
    {
        using TemporaryDirectory directory = new();
        using CryptoService cryptoService = new(CreateKey(directory));

        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => cryptoService.CreateRSAKeyPair(32768));

        Assert.Contains("must be less than", exception.Message);
    }

    [Fact]
    public void CreateRSAKeyPair_private_key_can_sign_data_verified_by_public_key()
    {
        using TemporaryDirectory directory = new();
        using CryptoService cryptoService = new(CreateKey(directory));
        RSAKeyPairResult keyPair = cryptoService.CreateRSAKeyPair();
        byte[] data = "SFTP authentication challenge"u8.ToArray();

        using RSA privateKey = RSA.Create();
        using RSA publicKey = RSA.Create();
        privateKey.ImportRSAPrivateKey(keyPair.PrivateKey, out _);
        publicKey.ImportRSAPublicKey(keyPair.PublicKey, out _);

        byte[] signature = privateKey.SignData(
            data,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        Assert.True(publicKey.VerifyData(
            data,
            signature,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1));
    }

    [Fact]
    public void RSA_key_pair_can_be_encrypted_for_storage_and_restored()
    {
        using TemporaryDirectory directory = new();
        using CryptoService cryptoService = new(CreateKey(directory));
        RSAKeyPairResult generatedKeyPair = cryptoService.CreateRSAKeyPair();

        byte[] encryptedPublicKey = cryptoService.Encrypt(generatedKeyPair.PublicKey);
        byte[] encryptedPrivateKey = cryptoService.Encrypt(generatedKeyPair.PrivateKey);
        RSAKeyPairResult restoredKeyPair = new(
            cryptoService.Decrypt(encryptedPublicKey),
            cryptoService.Decrypt(encryptedPrivateKey));

        Assert.Equal(generatedKeyPair.PublicKey, restoredKeyPair.PublicKey);
        Assert.Equal(generatedKeyPair.PrivateKey, restoredKeyPair.PrivateKey);
    }

    [Fact]
    public void Decrypt_rejects_a_modified_payload()
    {
        using TemporaryDirectory directory = new();
        string keyPath = CreateKey(directory);
        using CryptoService cryptoService = new(keyPath);
        byte[] payload = cryptoService.Encrypt("Sensitive file contents"u8.ToArray());
        payload[^1] ^= 0x01;

        Assert.Throws<AuthenticationTagMismatchException>(() => cryptoService.Decrypt(payload));
    }

    [Fact]
    public void Decrypt_rejects_a_payload_without_nonce_and_tag()
    {
        using TemporaryDirectory directory = new();
        string keyPath = CreateKey(directory);
        using CryptoService cryptoService = new(keyPath);

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => cryptoService.Decrypt(new byte[27]));

        Assert.Contains("too short", exception.Message);
    }

    [Fact]
    public void Encrypt_rejects_empty_plaintext()
    {
        using TemporaryDirectory directory = new();
        string keyPath = CreateKey(directory);
        using CryptoService cryptoService = new(keyPath);

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => cryptoService.Encrypt([]));

        Assert.Contains("cannot be empty", exception.Message);
    }

    [Fact]
    public void Encrypt_and_Decrypt_reject_null_arguments()
    {
        using TemporaryDirectory directory = new();
        string keyPath = CreateKey(directory);
        using CryptoService cryptoService = new(keyPath);

        ArgumentNullException encryptException = Assert.Throws<ArgumentNullException>(
            () => cryptoService.Encrypt((byte[])null!));
        Assert.Equal("plaintext", encryptException.ParamName);

        ArgumentNullException decryptException = Assert.Throws<ArgumentNullException>(
            () => cryptoService.Decrypt((byte[])null!));
        Assert.Equal("payload", decryptException.ParamName);
    }

    [Fact]
    public void Constructor_rejects_a_missing_key_file()
    {
        using TemporaryDirectory directory = new();
        string keyPath = Path.Combine(directory.Path, "missing-key.bin");

        FileNotFoundException exception = Assert.Throws<FileNotFoundException>(
            () => new CryptoService(keyPath));

        Assert.Contains(keyPath, exception.Message);
    }

    [Fact]
    public void Constructor_rejects_a_key_with_an_invalid_length()
    {
        using TemporaryDirectory directory = new();
        string keyPath = Path.Combine(directory.Path, "invalid-key.bin");
        File.WriteAllBytes(keyPath, new byte[16]);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => new CryptoService(keyPath));

        Assert.Contains("32 bytes", exception.Message);
    }

    private static string CreateKey(TemporaryDirectory directory)
    {
        string keyPath = Path.Combine(directory.Path, "encryption_key.bin");
        CryptoService.CreateKey(keyPath);
        return keyPath;
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = Directory.CreateTempSubdirectory("fileodon-crypto-tests-").FullName;
        }

        public string Path { get; }

        public void Dispose()
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
