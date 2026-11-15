using Domain.Interface;
using Domain.Service;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Test;

public sealed class UserOrganizationServiceTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetById_returns_null_for_non_positive_ids(int id)
    {
        Mock<IUserOrganizationRepository> repository = new();
        UserOrganizationService service = CreateService(
            CreateUnitOfWork(repository),
            repository,
            new Mock<IRoleRepository>(),
            CreateUserManager());

        UserOrganization? result = await service.GetByIdAsync(id);

        Assert.Null(result);
        repository.Verify(item => item.GetByIdWithRelationshipsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetById_returns_membership_with_relationships()
    {
        UserOrganization membership = CreateMembership();
        Mock<IUserOrganizationRepository> repository = new();
        repository
            .Setup(item => item.GetByIdWithRelationshipsAsync(membership.Id))
            .ReturnsAsync(membership);
        UserOrganizationService service = CreateService(
            CreateUnitOfWork(repository),
            repository,
            new Mock<IRoleRepository>(),
            CreateUserManager());

        UserOrganization? result = await service.GetByIdAsync(membership.Id);

        Assert.Same(membership, result);
    }

    [Fact]
    public async Task Delete_throws_when_membership_does_not_exist()
    {
        Mock<IUserOrganizationRepository> repository = new();
        repository
            .Setup(item => item.GetByIdAsync(42))
            .ReturnsAsync((UserOrganization?)null);
        UserOrganizationService service = CreateService(
            CreateUnitOfWork(repository),
            repository,
            new Mock<IRoleRepository>(),
            CreateUserManager());

        InvalidOperationException exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(42));

        Assert.Equal("UserOrganization not found", exception.Message);
        repository.Verify(item => item.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Delete_removes_existing_membership()
    {
        UserOrganization membership = CreateMembership();
        Mock<IUserOrganizationRepository> repository = new();
        repository
            .Setup(item => item.GetByIdAsync(membership.Id))
            .ReturnsAsync(membership);
        repository
            .Setup(item => item.DeleteAsync(membership.Id))
            .ReturnsAsync(membership);
        UserOrganizationService service = CreateService(
            CreateUnitOfWork(repository),
            repository,
            new Mock<IRoleRepository>(),
            CreateUserManager());

        UserOrganization? result = await service.DeleteAsync(membership.Id);

        Assert.Same(membership, result);
        repository.Verify(item => item.DeleteAsync(membership.Id), Times.Once);
    }

    [Fact]
    public async Task CreateUserOrganization_throws_when_user_does_not_exist()
    {
        Mock<UserManager<User>> userManager = CreateUserManager();
        userManager
            .Setup(manager => manager.FindByEmailAsync("member@example.com"))
            .ReturnsAsync((User?)null);
        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork(new Mock<IUserOrganizationRepository>());
        UserOrganizationService service = CreateService(
            unitOfWork,
            new Mock<IUserOrganizationRepository>(),
            new Mock<IRoleRepository>(),
            userManager);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateUserOrganizationAsync("member@example.com", 42, "role-id"));

        Assert.Equal("User not found", exception.Message);
        unitOfWork.Verify(work => work.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateUserOrganization_saves_membership_and_commits_transaction()
    {
        const string email = "member@example.com";
        const string userId = "member-id";
        UserOrganization? postedMembership = null;
        Mock<IUserOrganizationRepository> repository = new();
        repository
            .Setup(item => item.PostAsync(It.IsAny<UserOrganization>()))
            .Callback<UserOrganization>(membership => postedMembership = membership)
            .ReturnsAsync((UserOrganization membership) => membership);
        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork(repository);
        Mock<UserManager<User>> userManager = CreateUserManager();
        userManager
            .Setup(manager => manager.FindByEmailAsync(email))
            .ReturnsAsync(new User { Id = userId, Email = email });
        UserOrganizationService service = CreateService(
            unitOfWork,
            repository,
            new Mock<IRoleRepository>(),
            userManager);

        UserOrganization result =
            await service.CreateUserOrganizationAsync(email, 42, "role-id");

        Assert.Same(postedMembership, result);
        Assert.NotNull(postedMembership);
        Assert.Equal(userId, postedMembership.UserId);
        Assert.Equal(42, postedMembership.OrganizationId);
        Assert.Equal("role-id", postedMembership.RoleId);
        unitOfWork.Verify(work => work.BeginTransactionAsync(), Times.Once);
        unitOfWork.Verify(work => work.CommitAsync(), Times.Once);
        unitOfWork.Verify(work => work.RollbackAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateUserOrganization_rolls_back_when_saving_membership_fails()
    {
        Mock<IUserOrganizationRepository> repository = new();
        repository
            .Setup(item => item.PostAsync(It.IsAny<UserOrganization>()))
            .ThrowsAsync(new InvalidOperationException("Save failed"));
        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork(repository);
        Mock<UserManager<User>> userManager = CreateUserManager();
        userManager
            .Setup(manager => manager.FindByEmailAsync("member@example.com"))
            .ReturnsAsync(new User { Id = "member-id" });
        UserOrganizationService service = CreateService(
            unitOfWork,
            repository,
            new Mock<IRoleRepository>(),
            userManager);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateUserOrganizationAsync("member@example.com", 42, "role-id"));

        Assert.Equal("Save failed", exception.Message);
        unitOfWork.Verify(work => work.RollbackAsync(), Times.Once);
        unitOfWork.Verify(work => work.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateUserOrganization_rolls_back_when_committing_transaction_fails()
    {
        Mock<IUserOrganizationRepository> repository = new();
        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork(repository);
        unitOfWork
            .Setup(work => work.CommitAsync())
            .ThrowsAsync(new InvalidOperationException("Commit failed"));
        Mock<UserManager<User>> userManager = CreateUserManager();
        userManager
            .Setup(manager => manager.FindByEmailAsync("member@example.com"))
            .ReturnsAsync(new User { Id = "member-id" });
        UserOrganizationService service = CreateService(
            unitOfWork,
            repository,
            new Mock<IRoleRepository>(),
            userManager);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateUserOrganizationAsync("member@example.com", 42, "role-id"));

        Assert.Equal("Commit failed", exception.Message);
        unitOfWork.Verify(work => work.BeginTransactionAsync(), Times.Once);
        unitOfWork.Verify(work => work.CommitAsync(), Times.Once);
        unitOfWork.Verify(work => work.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByOrganizationId_returns_repository_results()
    {
        List<UserOrganization> memberships = [CreateMembership()];
        Mock<IUserOrganizationRepository> repository = new();
        repository
            .Setup(item => item.GetUserOrganizationsByOrganizationIdAsync(42))
            .ReturnsAsync(memberships);
        UserOrganizationService service = CreateService(
            CreateUnitOfWork(repository),
            repository,
            new Mock<IRoleRepository>(),
            CreateUserManager());

        IEnumerable<UserOrganization> result = await service.GetByOrganizationIdAsync(42);

        Assert.Same(memberships, result);
    }

    [Fact]
    public async Task GetByUserId_returns_repository_results()
    {
        const string userId = "member-id";
        List<UserOrganization> memberships = [CreateMembership()];
        Mock<IUserOrganizationRepository> repository = new();
        repository
            .Setup(item => item.GetUserOrganizationsByUseridAsync(userId))
            .ReturnsAsync(memberships);
        UserOrganizationService service = CreateService(
            CreateUnitOfWork(repository),
            repository,
            new Mock<IRoleRepository>(),
            CreateUserManager());

        IEnumerable<UserOrganization> result = await service.GetByUserIdAsync(userId);

        Assert.Same(memberships, result);
    }

    [Fact]
    public async Task EditUserOrganization_throws_when_membership_does_not_exist()
    {
        const int userOrganizationId = 42;
        const int organizationId = 24;
        const string roleId = "new-role";
        Mock<IUserOrganizationRepository> repository = new();
        repository
            .Setup(item => item.GetByIdAsync(userOrganizationId))
            .ReturnsAsync((UserOrganization?)null);
        UserOrganizationService service = CreateService(
            CreateUnitOfWork(repository),
            repository,
            new Mock<IRoleRepository>(),
            CreateUserManager());

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.EditUserOrganizationAsync(userOrganizationId, organizationId, roleId));

        Assert.Equal("UserOrganization not found", exception.Message);
    }

    [Fact]
    public async Task EditUserOrganization_throws_when_role_does_not_exist()
    {
        const int userOrganizationId = 42;
        const int organizationId = 24;
        const string roleId = "non-existent-role";
        Mock<IUserOrganizationRepository> repository = new();
        repository
            .Setup(item => item.GetByIdAsync(42))
            .ReturnsAsync(CreateMembership());
        Mock<IRoleRepository> roleRepository = new();
        roleRepository
            .Setup(item => item.GetByIdAsync(organizationId, roleId))
            .ReturnsAsync((AspNetRole?)null);
        UserOrganizationService service = CreateService(
            CreateUnitOfWork(repository),
            repository,
            roleRepository,
            CreateUserManager());

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.EditUserOrganizationAsync(userOrganizationId, organizationId, roleId));

        Assert.Equal("Role not found", exception.Message);
        repository.Verify(item => item.PutAsync(It.IsAny<UserOrganization>()), Times.Never);
    }

    [Fact]
    public async Task EditUserOrganization_updates_role_and_returns_membership_with_relationships()
    {
        const int organizationId = 24;
        const string roleId = "new-role";
        UserOrganization membership = CreateMembership();
        UserOrganization updatedMembership = CreateMembership();
        updatedMembership.RoleId = roleId;
        Mock<IUserOrganizationRepository> repository = new();
        repository
            .Setup(item => item.GetByIdAsync(membership.Id))
            .ReturnsAsync(membership);
        repository
            .Setup(item => item.PutAsync(membership))
            .ReturnsAsync(membership);
        repository
            .Setup(item => item.GetByIdWithRelationshipsAsync(membership.Id))
            .ReturnsAsync(updatedMembership);
        Mock<IRoleRepository> roleRepository = new();
        roleRepository
            .Setup(item => item.GetByIdAsync(organizationId, roleId))
            .ReturnsAsync(new AspNetRole { Id = roleId });
        UserOrganizationService service = CreateService(
            CreateUnitOfWork(repository),
            repository,
            roleRepository,
            CreateUserManager());

        UserOrganization result = await service.EditUserOrganizationAsync(membership.Id, organizationId, roleId);

        Assert.Equal("new-role", membership.RoleId);
        Assert.Same(updatedMembership, result);
        repository.Verify(item => item.PutAsync(membership), Times.Once);
    }

    [Fact]
    public async Task EditUserOrganization_throws_when_updated_membership_cannot_be_reloaded()
    {
        const int organizationId = 24;
        const string roleId = "new-role";
        UserOrganization membership = CreateMembership();
        Mock<IUserOrganizationRepository> repository = new();
        repository
            .Setup(item => item.GetByIdAsync(membership.Id))
            .ReturnsAsync(membership);
        repository
            .Setup(item => item.PutAsync(membership))
            .ReturnsAsync(membership);
        repository
            .Setup(item => item.GetByIdWithRelationshipsAsync(membership.Id))
            .ReturnsAsync((UserOrganization?)null);
        Mock<IRoleRepository> roleRepository = new();
        roleRepository
            .Setup(item => item.GetByIdAsync(organizationId, roleId))
            .ReturnsAsync(new AspNetRole { Id = roleId });
        UserOrganizationService service = CreateService(
            CreateUnitOfWork(repository),
            repository,
            roleRepository,
            CreateUserManager());

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.EditUserOrganizationAsync(membership.Id, organizationId, roleId));

        Assert.Equal("Could not find User organization after update", exception.Message);
    }

    private static UserOrganizationService CreateService(
        Mock<IUnitOfWork> unitOfWork,
        Mock<IUserOrganizationRepository> repository,
        Mock<IRoleRepository> roleRepository,
        Mock<UserManager<User>> userManager) =>
        new(unitOfWork.Object, repository.Object, roleRepository.Object, userManager.Object);

    private static Mock<IUnitOfWork> CreateUnitOfWork(
        Mock<IUserOrganizationRepository> repository)
    {
        Mock<IUnitOfWork> unitOfWork = new();
        unitOfWork
            .SetupGet(work => work.UserOrganizationRepository)
            .Returns(repository.Object);
        unitOfWork
            .Setup(work => work.BeginTransactionAsync())
            .Returns(Task.CompletedTask);
        unitOfWork
            .Setup(work => work.CommitAsync())
            .Returns(Task.CompletedTask);
        unitOfWork
            .Setup(work => work.RollbackAsync())
            .Returns(Task.CompletedTask);
        return unitOfWork;
    }

    private static Mock<UserManager<User>> CreateUserManager() =>
        new(
            Mock.Of<IUserStore<User>>(),
            Options.Create(new IdentityOptions()),
            Mock.Of<IPasswordHasher<User>>(),
            Array.Empty<IUserValidator<User>>(),
            Array.Empty<IPasswordValidator<User>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<User>>>());

    private static UserOrganization CreateMembership() =>
        new()
        {
            Id = 42,
            UserId = "member-id",
            OrganizationId = 24,
            RoleId = "old-role"
        };
}
