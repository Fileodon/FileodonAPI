using System.Security.Cryptography;
using Domain.Interface;

namespace Domain.Service;

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

    public byte[] Encrypt(byte[] plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext, nameof(plaintext));
        if (plaintext.Length == 0)
        {
            throw new ArgumentException("Plaintext cannot be null or empty.", nameof(plaintext));
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

        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new InvalidOperationException("Invalid path"));

        var fsOptions = new FileStreamOptions
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.Write,
        };

        if (!OperatingSystem.IsWindows())
        {
            fsOptions.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite; // Set permissions to 600 on Linux/MacOS
        }

        using var fs = new FileStream(path, fsOptions);

        fs.Write(newKey);

        return newKey;
    }

    public void Dispose()
    {
        _aes.Dispose();
        GC.SuppressFinalize(this);
    }
}
