using Domain.DTO;
using Domain.Enum;
using Domain.Interface;
using Domain.Service;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
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
            new MutationAuthorizationService<SFTPConnectionAuthCode>(),
            Mock.Of<ICryptoService>());

        MutationResult<SFTPConnectionAuthCode> result =
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
}
