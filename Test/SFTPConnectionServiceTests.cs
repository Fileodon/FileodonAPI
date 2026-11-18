using Domain.DTO;
using Domain.Enum;
using Domain.Interface;
using Domain.Service;
using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Infrastructure.Models.DTO;
using Infrastructure.Models.Enums;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Test;

public sealed class SFTPConnectionServiceTests
{
    [Fact]
    public async Task GenerateVerificationCode_saves_the_auth_code_before_committing()
    {
        const int organizationId = 42;
        const string userId = "037918b6-4687-4f33-8209-78cb27cba577";
        Mock<IUnitOfWork> unitOfWork = new();
        Mock<ISFTPConnectionAuthCodeRepository> authCodeRepository = new();
        SFTPConnectionAuthCode? addedAuthCode = null;
        authCodeRepository
            .Setup(repository => repository.AddAuthCodeAsync(
                It.IsAny<string>(),
                organizationId,
                userId))
            .Callback<string, int, string>((code, _, _) =>
            {
                addedAuthCode = new SFTPConnectionAuthCode
                {
                    VerificationCode = code,
                    OrganizationId = organizationId,
                    UserId = userId
                };
            })
            .ReturnsAsync(() => addedAuthCode);
        unitOfWork
            .SetupGet(work => work.SFTPConnectionAuthCodeRepository)
            .Returns(authCodeRepository.Object);
        unitOfWork
            .Setup(work => work.SaveChangesAsync())
            .ReturnsAsync(1);
        SFTPConnectionService service = new(
            unitOfWork.Object,
            Mock.Of<ICryptoService>(),
            Mock.Of<IConfiguration>());

        MutationEnvelope<SFTPConnectionAuthCode> result =
            await service.GenerateVerificationCodeAsync(organizationId, userId);

        Assert.Equal(MutationStatus.Success, result.Status);
        Assert.NotNull(result.Data);
        Assert.NotNull(addedAuthCode);
        Assert.Equal(8, addedAuthCode.VerificationCode.Length);
        Assert.Matches("^[A-Z0-9]{8}$", addedAuthCode.VerificationCode);
        unitOfWork.Verify(work => work.BeginTransactionAsync(), Times.Once);
        unitOfWork.Verify(work => work.SaveChangesAsync(), Times.Once);
        unitOfWork.Verify(work => work.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateSFTPConnectionStatus_accepts_connection_and_generates_encrypted_keys()
    {
        const string userId = "037918b6-4687-4f33-8209-78cb27cba577";
        const int organizationId = 42;
        SFTPConnection connection = new()
        {
            Id = 7,
            DisplayName = "Production",
            Host = "sftp.example.com",
            Port = 22,
            Status = SFTPConnectionStatus.Pending
        };
        Mock<ISFTPConnectionRepository> repository = new();
        repository
            .Setup(item => item.GetSFTPConnectionByIdAsync(connection.Id, organizationId, userId, false))
            .ReturnsAsync(connection);
        repository
            .Setup(item => item.AcceptSFTPConnection(connection))
            .Callback(() => connection.Status = SFTPConnectionStatus.Accepted)
            .Returns(connection);
        Mock<ICryptoService> cryptoService = new();
        cryptoService
            .Setup(item => item.CreateRSAKeyPair(It.IsAny<int>()))
            .Returns(new RSAKeyPairResult([1], [2]));
        cryptoService
            .Setup(item => item.Encrypt(It.IsAny<byte[]>()))
            .Returns<byte[]>(value => value);
        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork(repository);
        SFTPConnectionService service = CreateService(unitOfWork, cryptoService.Object);

        MutationEnvelope<SFTPConnection> result =
            await service.UpdateSFTPConnectionStatusAsync(connection.Id, organizationId, userId, SFTPConnectionStatus.Accepted);

        Assert.Equal(MutationStatus.Success, result.Status);
        Assert.Same(connection, result.Data);
        Assert.Equal(SFTPConnectionStatus.Accepted, connection.Status);
        Assert.Equal(Convert.ToBase64String([1]), connection.PublicKey);
        Assert.Equal(Convert.ToBase64String([2]), connection.PrivateKey);
        unitOfWork.Verify(work => work.SaveChangesAsync(), Times.Once);
        unitOfWork.Verify(work => work.CommitAsync(), Times.Once);
        unitOfWork.Verify(work => work.RollbackAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateSFTPConnectionStatus_rejects_connection_and_clears_existing_keys()
    {
        const string userId = "037918b6-4687-4f33-8209-78cb27cba577";
        const int organizationId = 42;
        SFTPConnection connection = new()
        {
            Id = 8,
            DisplayName = "Production",
            Host = "sftp.example.com",
            Port = 22,
            Status = SFTPConnectionStatus.Accepted,
            PublicKey = "public-key",
            PrivateKey = "private-key"
        };
        Mock<ISFTPConnectionRepository> repository = new();
        repository
            .Setup(item => item.GetSFTPConnectionByIdAsync(connection.Id, organizationId, userId, false))
            .ReturnsAsync(connection);
        repository
            .Setup(item => item.RejectSFTPConnection(connection))
            .Callback(() => connection.Status = SFTPConnectionStatus.Rejected)
            .Returns(connection);
        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork(repository);
        SFTPConnectionService service = CreateService(unitOfWork, Mock.Of<ICryptoService>());

        MutationEnvelope<SFTPConnection> result =
            await service.UpdateSFTPConnectionStatusAsync(connection.Id, organizationId, userId, SFTPConnectionStatus.Rejected);

        Assert.Equal(MutationStatus.Success, result.Status);
        Assert.Null(connection.PublicKey);
        Assert.Null(connection.PrivateKey);
        Assert.Equal(SFTPConnectionStatus.Rejected, connection.Status);
        unitOfWork.Verify(work => work.SaveChangesAsync(), Times.Once);
        unitOfWork.Verify(work => work.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateSFTPConnectionStatus_returns_not_found_when_user_cannot_access_connection()
    {
        const string userId = "037918b6-4687-4f33-8209-78cb27cba577";
        const int organizationId = 42;
        Mock<ISFTPConnectionRepository> repository = new();
        repository
            .Setup(item => item.GetSFTPConnectionByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), false))
            .ReturnsAsync((SFTPConnection?)null);
        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork(repository);
        SFTPConnectionService service = CreateService(unitOfWork, Mock.Of<ICryptoService>());

        MutationEnvelope<SFTPConnection> result =
            await service.UpdateSFTPConnectionStatusAsync(7, organizationId, userId, SFTPConnectionStatus.Accepted);

        Assert.Equal(MutationStatus.NotFound, result.Status);
        Assert.Null(result.Data);
        unitOfWork.Verify(work => work.BeginTransactionAsync(), Times.Never);
        unitOfWork.Verify(work => work.SaveChangesAsync(), Times.Never);
    }

    private static SFTPConnectionService CreateService(
        Mock<IUnitOfWork> unitOfWork,
        ICryptoService cryptoService) =>
        new(
            unitOfWork.Object,
            cryptoService,
            Mock.Of<IConfiguration>());

    private static Mock<IUnitOfWork> CreateUnitOfWork(Mock<ISFTPConnectionRepository> repository)
    {
        Mock<IUnitOfWork> unitOfWork = new();
        unitOfWork
            .SetupGet(work => work.SFTPConnectionRepository)
            .Returns(repository.Object);
        unitOfWork
            .Setup(work => work.BeginTransactionAsync())
            .Returns(Task.CompletedTask);
        unitOfWork
            .Setup(work => work.SaveChangesAsync())
            .ReturnsAsync(1);
        unitOfWork
            .Setup(work => work.CommitAsync())
            .Returns(Task.CompletedTask);
        unitOfWork
            .Setup(work => work.RollbackAsync())
            .Returns(Task.CompletedTask);
        return unitOfWork;
    }
}
