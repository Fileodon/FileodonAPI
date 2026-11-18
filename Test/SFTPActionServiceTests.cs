using Domain.Service;
using Infrastructure.Interface.Repository;
using Infrastructure.Interface.Repository.SFTP;
using Infrastructure.Interface.Service;
using Infrastructure.Models;
using Infrastructure.Models.DTO.SFTPAction;
using Infrastructure.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using Renci.SshNet.Sftp;

namespace Test;

public sealed class SFTPActionServiceTests
{
    [Fact]
    public async Task Delete_delegates_to_repository()
    {
        SFTPConnection connection = CreateConnection();
        Mock<ISFTPRepository> repository = new();
        SFTPActionDeleteResult expected = new(StatusCode.Ok, ["/file.txt"]);
        repository
            .Setup(item => item.DeleteAsync(connection, "/file.txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        SFTPActionService service = CreateService(repository);

        SFTPActionDeleteResult result =
            await service.DeleteAsync(connection, "/file.txt", CancellationToken.None);

        Assert.Same(expected, result);
        repository.Verify(item => item.DeleteAsync(connection, "/file.txt", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateDirectory_delegates_to_repository()
    {
        SFTPConnection connection = CreateConnection();
        Mock<ISFTPRepository> repository = new();
        List<SFTPActionCreateDirectoryResult> expected = [new(StatusCode.Ok, "/new")];
        repository
            .Setup(item => item.CreateDirectoryAsync(connection, "/new", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        SFTPActionService service = CreateService(repository);

        List<SFTPActionCreateDirectoryResult> result =
            await service.CreateDirectoryAsync(connection, "/new", CancellationToken.None);

        Assert.Same(expected, result);
        repository.Verify(item => item.CreateDirectoryAsync(connection, "/new", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Download_delegates_to_repository()
    {
        SFTPConnection connection = CreateConnection();
        Mock<ISFTPRepository> repository = new();
        SFTPActionDownloadResult expected = new(StatusCode.Ok, Stream: new MemoryStream(), FileName: "file.txt");
        repository
            .Setup(item => item.DownloadFileAsync(connection, "/file.txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        SFTPActionService service = CreateService(repository);

        SFTPActionDownloadResult result =
            await service.DownloadFileAsync(connection, "/file.txt", CancellationToken.None);

        Assert.Same(expected, result);
        repository.Verify(item => item.DownloadFileAsync(connection, "/file.txt", It.IsAny<CancellationToken>()), Times.Once);
        expected.Stream!.Dispose();
    }

    [Fact]
    public async Task Rename_delegates_to_repository()
    {
        SFTPConnection connection = CreateConnection();
        Mock<ISFTPRepository> repository = new();
        SFTPActionRenameResult expected = new(StatusCode.Ok);
        repository
            .Setup(item => item.RenameFileAsync(connection, "/old.txt", "/new.txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        SFTPActionService service = CreateService(repository);

        SFTPActionRenameResult result =
            await service.RenameFileAsync(connection, "/old.txt", "/new.txt", CancellationToken.None);

        Assert.Same(expected, result);
        repository.Verify(item => item.RenameFileAsync(connection, "/old.txt", "/new.txt", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task List_delegates_to_repository()
    {
        SFTPConnection connection = CreateConnection();
        Mock<ISFTPRepository> repository = new();
        SFTPActionListResult expected = new([], StatusCode.Ok);
        repository
            .Setup(item => item.ListFilesAsync(connection, "/home", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        SFTPActionService service = CreateService(repository);

        SFTPActionListResult result =
            await service.ListFilesAsync(connection, "/home", CancellationToken.None);

        Assert.Same(expected, result);
        repository.Verify(item => item.ListFilesAsync(connection, "/home", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Exists_delegates_to_repository()
    {
        SFTPConnection connection = CreateConnection();
        Mock<ISFTPRepository> repository = new();
        SFTPActionExistsResult expected = new(StatusCode.NoSuchFile);
        repository
            .Setup(item => item.FileExistsAsync(connection, "/missing.txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        SFTPActionService service = CreateService(repository);

        SFTPActionExistsResult result =
            await service.FileExistsAsync(connection, "/missing.txt", CancellationToken.None);

        Assert.Same(expected, result);
        repository.Verify(item => item.FileExistsAsync(connection, "/missing.txt", It.IsAny<CancellationToken>()), Times.Once);
    }

    private static SFTPActionService CreateService(Mock<ISFTPRepository> repository)
    {
        Mock<IUserContextService> userContext = new();
        userContext
            .Setup(item => item.GetCurrentUserId())
            .Returns("test-user");

        return new(
            Mock.Of<ILogger<SFTPActionService>>(),
            repository.Object,
            Mock.Of<ISFTPUploadSender>(),
            Mock.Of<ISFTPDistributionJobRepository>(),
            userContext.Object,
            Mock.Of<ISFTPActionAuditLogRepository>());
    }

    private static SFTPConnection CreateConnection() =>
        new()
        {
            Host = "sftp.example.com"
        };
}
