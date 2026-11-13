using System.Buffers.Binary;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using Infrastructure.Interface;
using Infrastructure.Models.DTO;

namespace Infrastructure.Service;

public class CryptoService : ICryptoService, IDisposable
{
    private const int KeySize = 32; // 256 bits
    private const int NonceSize = 12; // 96 bits
    private const int TagSize = 16; // 128 bits

    private readonly AesGcm _aes;

    public CryptoService(string keyPath)
    {
        byte[] key;

        key = LoadKey(keyPath);

        if (key.Length != KeySize)
        {
            throw new InvalidOperationException($"Key must be {KeySize} bytes long.");
        }

        _aes = new AesGcm(key, TagSize);
    }

    /// <summary>
    /// Decrypts the given payload using AES-GCM.
    /// </summary>
    /// <param name="payload">The encrypted payload to decrypt. It must contain the nonce, ciphertext, and authentication tag.</param>
    /// <returns>The decrypted plaintext.</returns>
    /// <exception cref="ArgumentException"></exception>
    public byte[] Decrypt(byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));
        if (payload.Length < NonceSize + TagSize)
        {
            throw new ArgumentException("Payload is too short.", nameof(payload));
        }

        var nonce = payload.AsSpan(0, NonceSize);
        var tag = payload.AsSpan(NonceSize, TagSize);
        var ciphertext = payload.AsSpan(NonceSize + TagSize);
        var plaintext = new byte[ciphertext.Length];

        _aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return plaintext;
    }

    /// <summary>
    /// Encrypts the given plaintext using AES-GCM.
    /// </summary>
    /// <param name="plaintext">The plaintext to encrypt. It must not be null or empty.</param>
    /// <returns>The encrypted payload, which includes the nonce, ciphertext, and authentication tag.</returns>
    /// <exception cref="ArgumentException"></exception>
    public byte[] Encrypt(byte[] plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext, nameof(plaintext));
        if (plaintext.Length == 0)
        {
            throw new ArgumentException("Plaintext cannot be empty.", nameof(plaintext));
        }

        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        _aes.Encrypt(nonce, plaintext, ciphertext, tag);

        var result = new byte[NonceSize + ciphertext.Length + TagSize];
        nonce.CopyTo(result, 0);
        tag.CopyTo(result, NonceSize);
        ciphertext.CopyTo(result, NonceSize + TagSize);
        return result;
    }

    /// <summary>
    /// Encrypts the given plaintext string using AES-GCM and returns the result as a Base64-encoded string.
    /// </summary>
    /// <param name="plainText">The plaintext string to encrypt. It must not be null, empty, or whitespace.</param>
    /// <returns>The encrypted payload as a Base64-encoded string.</returns>
    /// <exception cref="ArgumentException"></exception>
    public string Encrypt(string plainText)
    {
        ArgumentNullException.ThrowIfNull(plainText, nameof(plainText));
        if (string.IsNullOrWhiteSpace(plainText))
        {
            throw new ArgumentException("Plaintext cannot be empty or whitespace.", nameof(plainText));
        }

        var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = Encrypt(plainBytes);
        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// Decrypts the given Base64-encoded payload string using AES-GCM and returns the result as a plaintext string.
    /// </summary>
    /// <param name="payload">The Base64-encoded encrypted payload string to decrypt. It must not be null, empty, or whitespace.</param>
    /// <returns>The decrypted plaintext string.</returns>
    /// <exception cref="ArgumentException"></exception>
    public string Decrypt(string payload)
    {
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException("Payload cannot be empty or whitespace.", nameof(payload));
        }

        var payloadBytes = Convert.FromBase64String(payload);
        var decryptedBytes = Decrypt(payloadBytes);
        return System.Text.Encoding.UTF8.GetString(decryptedBytes);
    }

    public RSAKeyPairResult CreateRSAKeyPair(int keySize = 4096)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(keySize, 2048, nameof(keySize));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(keySize, 16384, nameof(keySize)); // RSA theoretically has no limit, but we follow OpenSSL recommendations for practical key sizes.
        using var rsa = RSA.Create(keySize);
        var privateKey = rsa.ExportRSAPrivateKey();
        var publicKey = rsa.ExportRSAPublicKey();
        return new RSAKeyPairResult(
            publicKey,
            privateKey
        );
    }

    private static byte[] LoadKey(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Key file not found at path: {path}");
        }

        return File.ReadAllBytes(path);
    }

    public static byte[] CreateKey(string path)
    {
        if (File.Exists(path))
        {
            throw new InvalidOperationException($"Key file already exists at path: {path}");
        }

        var newKey = RandomNumberGenerator.GetBytes(KeySize);

        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);

        }

        var fsOptions = new FileStreamOptions
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.Write,
        };

        if (!OperatingSystem.IsWindows())
        {
            fsOptions.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite; // Set permissions to 600 on Linux/MacOS
        }

        using var fs = OperatingSystem.IsWindows()
                ? CreateRestrictedFile(path)
                : new FileStream(path, fsOptions);

        fs.Write(newKey);

        return newKey;
    }

    /// <summary>
    /// Creates a file with restricted access permissions on Windows.
    /// </summary>
    /// <param name="path">The path of the file to create.</param>
    /// <returns>A <see cref="FileStream"/> for the newly created file.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the operating system is not Windows.</exception>
    private static FileStream CreateRestrictedFile(string path)
    {
        if (!OperatingSystem.IsWindows())
            throw new InvalidOperationException("This method is only supported on Windows.");

        var security = new FileSecurity();
        security.SetAccessRuleProtection(true, false); // Protect the ACL from inheritance and remove existing inherited rules
        // Grant full control to the current user
        security.AddAccessRule(new FileSystemAccessRule(
            WindowsIdentity.GetCurrent().Name,
            FileSystemRights.FullControl,
            AccessControlType.Allow));
        // Grant read access to administrators group
        security.AddAccessRule(new FileSystemAccessRule(
            new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
            FileSystemRights.Read,
            AccessControlType.Allow));

        return FileSystemAclExtensions.Create(
            new FileInfo(path),
            FileMode.CreateNew,
            FileSystemRights.Write,
            FileShare.None,
            4096,
            FileOptions.None,
            security);
    }

    public void Dispose()
    {
        _aes.Dispose();
        GC.SuppressFinalize(this);
    }

    public string ConvertPublicKeyToPem(byte[] publicKey)
    {
        ArgumentNullException.ThrowIfNull(publicKey, nameof(publicKey));
        if (publicKey.Length == 0)
        {
            throw new ArgumentException("Public key bytes cannot be empty.", nameof(publicKey));
        }
        using RSA rsa = RSA.Create();
        rsa.ImportRSAPublicKey(publicKey, out _);

        string pem = rsa.ExportRSAPublicKeyPem();

        return pem;
    }

    public string ConvertPrivateKeyToPem(byte[] privateKey)
    {
        ArgumentNullException.ThrowIfNull(privateKey, nameof(privateKey));
        if (privateKey.Length == 0)
        {
            throw new ArgumentException("Private key bytes cannot be empty.", nameof(privateKey));
        }
        using RSA rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(privateKey, out _);

        string pem = rsa.ExportRSAPrivateKeyPem();

        return pem;
    }

    public string ConvertPublicKeyToOpenSsh(byte[] publicKey, string? comment = null)
    {
        using RSA rsa = RSA.Create();
        rsa.ImportRSAPublicKey(publicKey, out _);

        RSAParameters parameters = rsa.ExportParameters(includePrivateParameters: false);

        byte[] keyBlob = Combine(
            EncodeSshString("ssh-rsa"u8.ToArray()),
            EncodeSshMpint(parameters.Exponent!),
            EncodeSshMpint(parameters.Modulus!));

        string result = $"ssh-rsa {Convert.ToBase64String(keyBlob)}";

        return string.IsNullOrWhiteSpace(comment)
            ? result
            : $"{result} {comment}";
    }

    private static byte[] EncodeSshString(byte[] value)
    {
        byte[] result = new byte[sizeof(int) + value.Length];
        BinaryPrimitives.WriteUInt32BigEndian(result, (uint)value.Length);
        value.CopyTo(result, sizeof(int));
        return result;
    }

    private static byte[] EncodeSshMpint(byte[] value)
    {
        int firstNonZero = 0;

        while (firstNonZero < value.Length - 1 && value[firstNonZero] == 0)
        {
            firstNonZero++;
        }

        byte[] unsignedValue = value[firstNonZero..];

        if ((unsignedValue[0] & 0x80) != 0)
        {
            unsignedValue = [0, .. unsignedValue];
        }

        return EncodeSshString(unsignedValue);
    }

    private static byte[] Combine(params byte[][] arrays)
    {
        byte[] result = new byte[arrays.Sum(array => array.Length)];
        int offset = 0;

        foreach (byte[] array in arrays)
        {
            array.CopyTo(result, offset);
            offset += array.Length;
        }

        return result;
    }
}
