using System.Security.Claims;
using Controller.Controllers;
using Controller.DTO.SFTPAction;
using Domain.DTO;
using Domain.Enum;
using Domain.Interface;
using Infrastructure.Models;
using Infrastructure.Models.DTO.SFTPAction;
using Infrastructure.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Renci.SshNet.Sftp;

namespace Test;

public sealed class SFTPActionControllerTests
{
    private const int OrganizationId = 12;
    private const int ConnectionId = 34;
    private const string UserId = "user-id";

    [Fact]
    public async Task Delete_returns_mapped_result()
    {
        (SFTPActionController controller, Mock<ISFTPActionService> actionService) = CreateController();
        actionService
            .Setup(service => service.DeleteAsync(
                It.IsAny<SFTPConnection>(), "/file.txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync([new SFTPActionDeleteResult(StatusCode.Ok, "/file.txt")]);

        IActionResult result = await controller.DeleteFileAsync(OrganizationId, ConnectionId, "/file.txt");

        OkObjectResult response = Assert.IsType<OkObjectResult>(result);
        Assert.Single(Assert.IsType<IEnumerable<SFTPActionDeleteResponse>>(response.Value, false));
    }

    [Fact]
    public async Task CreateDirectory_returns_mapped_result()
    {
        (SFTPActionController controller, Mock<ISFTPActionService> actionService) = CreateController();
        actionService
            .Setup(service => service.CreateDirectoryAsync(
                It.IsAny<SFTPConnection>(), "/new", It.IsAny<CancellationToken>()))
            .ReturnsAsync([new SFTPActionCreateDirectoryResult(StatusCode.Ok, "/new")]);

        IActionResult result = await controller.CreateDirectoryAsync(
            OrganizationId,
            ConnectionId,
            new SFTPActionCreateDirectoryRequest { RemoteDirectoryPath = "/new" });

        OkObjectResult response = Assert.IsType<OkObjectResult>(result);
        Assert.Single(Assert.IsType<IEnumerable<SFTPActionCreateDirectoryResponse>>(response.Value, false));
    }

    [Fact]
    public async Task Download_returns_file_result()
    {
        (SFTPActionController controller, Mock<ISFTPActionService> actionService) = CreateController();
        MemoryStream stream = new();
        actionService
            .Setup(service => service.DownloadFileAsync(
                It.IsAny<SFTPConnection>(), "/file.txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SFTPActionDownloadResult(StatusCode.Ok, Stream: stream, FileName: "file.txt"));

        IActionResult result = await controller.DownloadFileAsync(OrganizationId, ConnectionId, "/file.txt");

        FileStreamResult response = Assert.IsType<FileStreamResult>(result);
        Assert.Same(stream, response.FileStream);
        Assert.Equal("file.txt", response.FileDownloadName);
    }

    [Fact]
    public async Task Rename_returns_mapped_result()
    {
        (SFTPActionController controller, Mock<ISFTPActionService> actionService) = CreateController();
        actionService
            .Setup(service => service.RenameFileAsync(
                It.IsAny<SFTPConnection>(), "/old.txt", "/new.txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SFTPActionRenameResult(StatusCode.Ok));

        IActionResult result = await controller.RenameFileAsync(
            OrganizationId,
            ConnectionId,
            new SFTPActionRenameRequest("/old.txt", "/new.txt"));

        OkObjectResult response = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<SFTPActionRenameResponse>(response.Value);
    }

    [Fact]
    public async Task List_returns_mapped_result()
    {
        (SFTPActionController controller, Mock<ISFTPActionService> actionService) = CreateController();
        actionService
            .Setup(service => service.ListFilesAsync(
                It.IsAny<SFTPConnection>(), "/home", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SFTPActionListResult([], StatusCode.Ok));

        IActionResult result = await controller.ListFilesAsync(
            OrganizationId,
            ConnectionId,
            new SFTPActionListFilesRequest("/home"));

        OkObjectResult response = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<SFTPActionListFilesResponse>(response.Value, false);
    }

    [Fact]
    public async Task Exists_returns_mapped_result()
    {
        (SFTPActionController controller, Mock<ISFTPActionService> actionService) = CreateController();
        actionService
            .Setup(service => service.FileExistsAsync(
                It.IsAny<SFTPConnection>(), "/file.txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SFTPActionExistsResult(StatusCode.NoSuchFile));

        IActionResult result = await controller.CheckFileExistsAsync(OrganizationId, ConnectionId, "/file.txt");

        OkObjectResult response = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<SFTPActionExistsResponse>(response.Value);
    }

    private static (SFTPActionController Controller, Mock<ISFTPActionService> ActionService) CreateController()
    {
        Mock<ISFTPConnectionService> connectionService = new();
        connectionService
            .Setup(service => service.GetSFTPConnectionAsync(ConnectionId, OrganizationId, UserId))
            .ReturnsAsync(new MutationEnvelope<SFTPConnection>(
                MutationStatus.Success,
                new SFTPConnection
                {
                    Id = ConnectionId,
                    Host = "sftp.example.com",
                    Status = SFTPConnectionStatus.Accepted
                }));
        Mock<ISFTPActionService> actionService = new();
        SFTPActionController controller = new(connectionService.Object, actionService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity([new Claim("id", UserId)], "test"))
                }
            }
        };
        return (controller, actionService);
    }
}
