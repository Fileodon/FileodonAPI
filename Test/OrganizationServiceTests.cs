using Domain.DTO;
using Domain.Enum;
using Domain.Interface;
using Domain.Service;
using Infrastructure.Models;
using Moq;

namespace Test;

public sealed class OrganizationServiceTests
{
    [Fact]
    public async Task CreateOrganization_creates_an_organization_for_the_requesting_user()
    {
        const string userId = "owner-id";
        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork();
        Organization? postedOrganization = null;
        unitOfWork
            .Setup(work => work.OrganizationRepository.PostAsync(It.IsAny<Organization>()))
            .Callback<Organization>(organization => postedOrganization = organization)
            .ReturnsAsync((Organization organization) =>
            {
                organization.Id = 42;
                return organization;
            });
        OrganizationService service = new(unitOfWork.Object);

        Organization result = await service.CreateOrganizationAsync(
            "Acme",
            "acme.example.com",
            "owner@acme.example.com",
            userId);

        Assert.Equal(42, result.Id);
        Assert.NotNull(postedOrganization);
        Assert.Equal(userId, postedOrganization.UserOwnerId);
        Assert.Equal("Acme", postedOrganization.OrgName);
        Assert.Equal("acme.example.com", postedOrganization.WorkspaceURL);
        Assert.Equal("owner@acme.example.com", postedOrganization.ContactEmail);
        unitOfWork.Verify(work => work.BeginTransactionAsync(), Times.Once);
        unitOfWork.Verify(work => work.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Owner_can_update_an_organization()
    {
        const string userId = "owner-id";
        Organization organization = CreateOrganization(userId);
        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork();
        unitOfWork
            .Setup(work => work.OrganizationRepository.GetByIdAsync(organization.Id, userId))
            .ReturnsAsync(organization);
        unitOfWork
            .Setup(work => work.OrganizationRepository.UpdateOrganizationAsync(
                organization.Id,
                "Updated name",
                userId))
            .ReturnsAsync(organization);
        OrganizationService service = new(unitOfWork.Object);

        MutationResult<Organization> result =
            await service.UpdateOrganizationAsync(organization.Id, "Updated name", userId);

        Assert.Equal(MutationStatus.Success, result.Status);
        Assert.Same(organization, result.Data);
        unitOfWork.Verify(work => work.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Owner_can_delete_an_organization()
    {
        const string userId = "owner-id";
        Organization organization = CreateOrganization(userId);
        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork();
        unitOfWork
            .Setup(work => work.OrganizationRepository.GetByIdAsync(organization.Id, userId))
            .ReturnsAsync(organization);
        unitOfWork
            .Setup(work => work.OrganizationRepository.DeleteByIdAsync(organization.Id, userId))
            .ReturnsAsync(organization);
        OrganizationService service = new(unitOfWork.Object);

        MutationResult<Organization> result =
            await service.DeleteOrganizationByIdAsync(organization.Id, userId);

        Assert.Equal(MutationStatus.Success, result.Status);
        Assert.Same(organization, result.Data);
        unitOfWork.Verify(work => work.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Non_owner_cannot_update_an_organization()
    {
        const string userId = "member-id";
        Organization organization = CreateOrganization("owner-id");
        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork();
        unitOfWork
            .Setup(work => work.OrganizationRepository.GetByIdAsync(organization.Id, userId))
            .ReturnsAsync(organization);
        OrganizationService service = new(unitOfWork.Object);

        MutationResult<Organization> result =
            await service.UpdateOrganizationAsync(organization.Id, "Updated name", userId);

        Assert.Equal(MutationStatus.Forbidden, result.Status);
        Assert.Null(result.Data);
        unitOfWork.Verify(
            work => work.OrganizationRepository.UpdateOrganizationAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Non_owner_cannot_delete_an_organization()
    {
        const string userId = "member-id";
        Organization organization = CreateOrganization("owner-id");
        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork();
        unitOfWork
            .Setup(work => work.OrganizationRepository.GetByIdAsync(organization.Id, userId))
            .ReturnsAsync(organization);
        OrganizationService service = new(unitOfWork.Object);

        MutationResult<Organization> result =
            await service.DeleteOrganizationByIdAsync(organization.Id, userId);

        Assert.Equal(MutationStatus.Forbidden, result.Status);
        Assert.Null(result.Data);
        unitOfWork.Verify(
            work => work.OrganizationRepository.DeleteByIdAsync(
                It.IsAny<int>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Non_member_cannot_get_an_organization()
    {
        const string userId = "non-member-id";
        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork();
        unitOfWork
            .Setup(work => work.OrganizationRepository.GetByIdAsync(42, userId))
            .ReturnsAsync((Organization?)null);
        OrganizationService service = new(unitOfWork.Object);

        Organization? result = await service.GetOrganizationByIdAsync(42, userId);

        Assert.Null(result);
    }

    [Fact]
    public async Task Non_member_cannot_update_or_delete_an_organization()
    {
        const string userId = "non-member-id";
        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork();
        unitOfWork
            .Setup(work => work.OrganizationRepository.GetByIdAsync(42, userId))
            .ReturnsAsync((Organization?)null);
        OrganizationService service = new(unitOfWork.Object);

        MutationResult<Organization> updateResult =
            await service.UpdateOrganizationAsync(42, "Updated name", userId);
        MutationResult<Organization> deleteResult =
            await service.DeleteOrganizationByIdAsync(42, userId);

        Assert.Equal(MutationStatus.NotFound, updateResult.Status);
        Assert.Equal(MutationStatus.NotFound, deleteResult.Status);
        unitOfWork.Verify(
            work => work.OrganizationRepository.UpdateOrganizationAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
        unitOfWork.Verify(
            work => work.OrganizationRepository.DeleteByIdAsync(
                It.IsAny<int>(),
                It.IsAny<string>()),
            Times.Never);
    }

    private static Mock<IUnitOfWork> CreateUnitOfWork()
    {
        Mock<IUnitOfWork> unitOfWork = new();
        unitOfWork
            .Setup(work => work.DisableAutomaticTransactions())
            .Returns(Mock.Of<IDisposable>());
        return unitOfWork;
    }

    private static Organization CreateOrganization(string ownerId) =>
        new()
        {
            Id = 42,
            UserOwnerId = ownerId,
            OrgName = "Acme",
            WorkspaceURL = "acme.example.com",
            ContactEmail = "owner@acme.example.com"
        };
}
