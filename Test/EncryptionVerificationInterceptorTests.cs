using System.Security.Cryptography;
using Infrastructure;
using Infrastructure.Interceptors;
using Infrastructure.Models;
using Infrastructure.Models.DTO;
using Infrastructure.Service;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Test;

public sealed class EncryptionVerificationInterceptorTests
{
    [Fact]
    public void Saving_a_matching_encrypted_key_pair_succeeds()
    {
        using TestDatabase database = CreateDatabase();
        RSAKeyPairResult keyPair = CreateKeyPair(database.CryptoService);
        SFTPConnection connection = CreateConnection(
            Encrypt(database.CryptoService, keyPair.PublicKey),
            Encrypt(database.CryptoService, keyPair.PrivateKey));

        database.Context.SFTPConnections.Add(connection);

        database.Context.SaveChanges();

        Assert.NotEqual(0, connection.Id);
    }

    [Fact]
    public void Saving_a_plaintext_public_key_fails()
    {
        using TestDatabase database = CreateDatabase();
        RSAKeyPairResult keyPair = CreateKeyPair(database.CryptoService);
        SFTPConnection connection = CreateConnection(
            Convert.ToBase64String(keyPair.PublicKey),
            Encrypt(database.CryptoService, keyPair.PrivateKey));

        database.Context.SFTPConnections.Add(connection);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => database.Context.SaveChanges());

        Assert.Contains("public key is invalid", exception.Message);
    }

    [Fact]
    public void Saving_a_plaintext_private_key_fails()
    {
        using TestDatabase database = CreateDatabase();
        RSAKeyPairResult keyPair = CreateKeyPair(database.CryptoService);
        SFTPConnection connection = CreateConnection(
            Encrypt(database.CryptoService, keyPair.PublicKey),
            Convert.ToBase64String(keyPair.PrivateKey));

        database.Context.SFTPConnections.Add(connection);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => database.Context.SaveChanges());

        Assert.Contains("private key is invalid", exception.Message);
    }

    [Fact]
    public void Saving_encrypted_keys_from_different_key_pairs_fails()
    {
        using TestDatabase database = CreateDatabase();
        RSAKeyPairResult publicKeyPair = CreateKeyPair(database.CryptoService);
        RSAKeyPairResult privateKeyPair = CreateKeyPair(database.CryptoService);
        SFTPConnection connection = CreateConnection(
            Encrypt(database.CryptoService, publicKeyPair.PublicKey),
            Encrypt(database.CryptoService, privateKeyPair.PrivateKey));

        database.Context.SFTPConnections.Add(connection);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => database.Context.SaveChanges());

        Assert.Contains("public and private keys do not match", exception.Message);
    }

    [Fact]
    public void Removing_a_keypair_succeeds()
    {
        using TestDatabase database = CreateDatabase();
        RSAKeyPairResult keyPair = CreateKeyPair(database.CryptoService);
        SFTPConnection connection = CreateConnection(
            Encrypt(database.CryptoService, keyPair.PublicKey),
            Encrypt(database.CryptoService, keyPair.PrivateKey));

        database.Context.SFTPConnections.Add(connection);
        database.Context.SaveChanges();

        connection.PublicKey = null;
        connection.PrivateKey = null;

        database.Context.SaveChanges();

        Assert.Null(connection.PublicKey);
        Assert.Null(connection.PrivateKey);
    }

    private static RSAKeyPairResult CreateKeyPair(CryptoService cryptoService) =>
        cryptoService.CreateRSAKeyPair(2048);

    private static string Encrypt(CryptoService cryptoService, byte[] value) =>
        Convert.ToBase64String(cryptoService.Encrypt(value));

    private static SFTPConnection CreateConnection(string publicKey, string privateKey) =>
        new()
        {
            DisplayName = "Test connection",
            OrganizationId = 1,
            Host = "sftp.example.com",
            Port = 22,
            PublicKey = publicKey,
            PrivateKey = privateKey
        };

    private static TestDatabase CreateDatabase()
    {
        SqliteConnection connection = new("Data Source=:memory:");
        connection.Open();
        string keyPath = CreateEncryptionKeyPath();
        CryptoService cryptoService = new(keyPath);
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new EncryptionVerificationInterceptor(cryptoService))
            .Options;
        ApplicationDbContext context = new(options);
        context.Database.EnsureCreated();

        const string userId = "test-owner";
        context.Users.Add(new User
        {
            Id = userId,
            UserName = userId
        });
        context.Organizations.Add(new Organization
        {
            UserOwnerId = userId,
            OrgName = "Test organization",
            WorkspaceURL = "test.example.com",
            ContactEmail = "test@example.com"
        });
        context.SaveChanges();
        return new TestDatabase(context, connection, cryptoService, keyPath);
    }

    private static string CreateEncryptionKeyPath()
    {
        string path = Path.Combine(Path.GetTempPath(), $"fileodon-test-key-{Guid.NewGuid():N}");
        CryptoService.CreateKey(path);
        return path;
    }

    private sealed class TestDatabase(
        ApplicationDbContext context,
        SqliteConnection connection,
        CryptoService cryptoService,
        string keyPath) : IDisposable
    {
        public ApplicationDbContext Context { get; } = context;
        public CryptoService CryptoService { get; } = cryptoService;

        public void Dispose()
        {
            Context.Dispose();
            connection.Dispose();
            cryptoService.Dispose();
            File.Delete(keyPath);
        }
    }
}
