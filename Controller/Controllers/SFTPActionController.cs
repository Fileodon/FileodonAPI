using Controller.DTO.SFTPAction;
using Controller.Extensions;
using Controller.Middleware.Attribute;
using Domain.Enum;
using Domain.Interface;
using Infrastructure.Models.DTO.SFTPAction;
using Infrastructure.Models.Enums;
using Infrastructure.Worker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers
{
    [Route("organization/{organizationid}/[controller]/{sftpconnectionid}")]
    [ApiController]
    [Authorize]
    public class SFTPActionController : ControllerBase
    {
        // 10GB limit
        const long uploadLimit = 10_737_418_240;
        private readonly ISFTPConnectionService _sftpConnectionService;
        private readonly ISFTPActionService _sftpActionService;

        public SFTPActionController(ISFTPConnectionService sftpConnectionService, ISFTPActionService sftpActionService)
        {
            _sftpConnectionService = sftpConnectionService;
            _sftpActionService = sftpActionService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = uploadLimit)]
        [RequestSizeLimit(uploadLimit)]
        [ProducesResponseType(typeof(SFTPActionUploadResponse), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Permission(PermissionFlags.SftpActionCreate)]
        public async Task<IActionResult> UploadFileAsync([FromRoute] int organizationid, [FromRoute] int sftpconnectionid, [FromForm] SFTPActionUploadRequest uploadRequest)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            var connection = await _sftpConnectionService.GetSFTPConnectionAsync(sftpconnectionid, organizationid, userId);
            if (connection.Status == MutationStatus.NotFound || connection.Data is null)
            {
                return NotFound("SFTP connection not found.");
            }
            if (connection.Status == MutationStatus.Forbidden)
            {
                return Forbid("You do not have permission to access this SFTP connection.");
            }

            string[] remoteFilePaths = uploadRequest.GetRemoteFilePaths();
            if (!uploadRequest.OverrideExisting)
            {
                foreach (string remoteFilePath in remoteFilePaths)
                {
                    SFTPActionExistsResult existsResult = await _sftpActionService.FileExistsAsync(connection.Data, remoteFilePath, HttpContext.RequestAborted);
                    if (existsResult.StatusCode == Renci.SshNet.Sftp.StatusCode.Ok)
                    {
                        return Conflict($"File '{remoteFilePath}' already exists on the SFTP server.");
                    }
                    else if (existsResult.StatusCode != Renci.SshNet.Sftp.StatusCode.NoSuchFile)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, $"Error checking file existence: {existsResult.ErrorMessage}");
                    }
                }
            }

            List<SFTPUploadInput> files = uploadRequest.Files
                .Select((file, index) => new SFTPUploadInput(
                    file.OpenReadStream(),
                    remoteFilePaths[index],
                    file.Length))
                .ToList();
            try
            {
                Guid jobId = await _sftpActionService.EnqueueUploadAsync(connection.Data, files, HttpContext.RequestAborted);
                return Accepted(new SFTPActionUploadResponse(jobId));
            }
            finally
            {
                foreach (SFTPUploadInput file in files)
                {
                    await file.Content.DisposeAsync();
                }
            }
        }

        [HttpPost("create-directory")]
        [ProducesResponseType(typeof(List<SFTPActionCreateDirectoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Permission(PermissionFlags.SftpActionCreate)]
        public async Task<IActionResult> CreateDirectoryAsync([FromRoute] int organizationid, [FromRoute] int sftpconnectionid, [FromBody] SFTPActionCreateDirectoryRequest createRequest)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            var connection = await _sftpConnectionService.GetSFTPConnectionAsync(sftpconnectionid, organizationid, userId);
            if (connection.Status == MutationStatus.NotFound || connection.Data is null)
            {
                return NotFound("SFTP connection not found.");
            }
            if (connection.Status == MutationStatus.Forbidden)
            {
                return Forbid("You do not have permission to access this SFTP connection.");
            }

            List<SFTPActionCreateDirectoryResult> createDirectoryResults = await _sftpActionService.CreateDirectoryAsync(connection.Data, createRequest.RemoteDirectoryPath, HttpContext.RequestAborted);

            return Ok(createDirectoryResults.Select(result => result.ToResponse()));
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(IEnumerable<SFTPActionListFilesResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Permission(PermissionFlags.SftpActionRead)]
        public async Task<IActionResult> ListFilesAsync([FromRoute] int organizationid, [FromRoute] int sftpconnectionid, [FromQuery] SFTPActionListFilesRequest listRequest)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            var connection = await _sftpConnectionService.GetSFTPConnectionAsync(sftpconnectionid, organizationid, userId);
            if (connection.Status == MutationStatus.NotFound || connection.Data is null)
            {
                return NotFound("SFTP connection not found.");
            }
            if (connection.Status == MutationStatus.Forbidden)
            {
                return Forbid("You do not have permission to access this SFTP connection.");
            }

            SFTPActionListResult listResult = await _sftpActionService.ListFilesAsync(connection.Data, listRequest.RemoteDirectoryPath, HttpContext.RequestAborted);

            return Ok(listResult.FromSftpFile());
        }

        [HttpGet("upload/{jobId:guid}")]
        [ProducesResponseType(typeof(SFTPUploadJobSnapshot), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PollUpload(Guid jobId, [FromRoute] int organizationid, [FromRoute] int sftpconnectionid)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            var connection = await _sftpConnectionService.GetSFTPConnectionAsync(sftpconnectionid, organizationid, userId);
            if (connection.Status == MutationStatus.NotFound || connection.Data is null)
            {
                return NotFound("SFTP connection not found.");
            }
            if (connection.Status == MutationStatus.Forbidden)
            {
                return Forbid("You do not have permission to access this SFTP connection.");
            }

            SFTPUploadJobSnapshot? status = _sftpActionService.GetUploadStatus(jobId, connection.Data);
            return status is null ? NotFound("SFTP upload job not found.") : Ok(status);
        }

        [HttpGet("exists")]
        [ProducesResponseType(typeof(SFTPActionExistsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Permission(PermissionFlags.SftpActionRead)]
        public async Task<IActionResult> CheckFileExistsAsync([FromRoute] int organizationid, [FromRoute] int sftpconnectionid, [FromQuery] string remoteFilePath)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            var connection = await _sftpConnectionService.GetSFTPConnectionAsync(sftpconnectionid, organizationid, userId);
            if (connection.Status == MutationStatus.NotFound || connection.Data is null)
            {
                return NotFound("SFTP connection not found.");
            }
            if (connection.Status == MutationStatus.Forbidden)
            {
                return Forbid("You do not have permission to access this SFTP connection.");
            }

            SFTPActionExistsResult fileExists = await _sftpActionService.FileExistsAsync(connection.Data, remoteFilePath, HttpContext.RequestAborted);

            return Ok(fileExists.ToResponse());
        }

        [HttpGet("download")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Permission(PermissionFlags.SftpActionRead)]
        public async Task<IActionResult> DownloadFileAsync([FromRoute] int organizationid, [FromRoute] int sftpconnectionid, [FromQuery] string remoteFilePath)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            var connection = await _sftpConnectionService.GetSFTPConnectionAsync(sftpconnectionid, organizationid, userId);
            if (connection.Status == MutationStatus.NotFound || connection.Data is null)
            {
                return NotFound("SFTP connection not found.");
            }
            if (connection.Status == MutationStatus.Forbidden)
            {
                return Forbid("You do not have permission to access this SFTP connection.");
            }

            SFTPActionDownloadResult downloadResult = await _sftpActionService.DownloadFileAsync(connection.Data, remoteFilePath, HttpContext.RequestAborted);

            if (downloadResult.Stream is null)
            {
                return NotFound($"File '{remoteFilePath}' not found on the SFTP server.");
            }

            return File(downloadResult.Stream, "application/octet-stream", downloadResult.FileName ?? string.Empty);
        }

        [HttpPut("rename")]
        [ProducesResponseType(typeof(SFTPActionRenameResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Permission(PermissionFlags.SftpActionUpdate)]
        public async Task<IActionResult> RenameFileAsync([FromRoute] int organizationid, [FromRoute] int sftpconnectionid, [FromBody] SFTPActionRenameRequest renameRequest)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            var connection = await _sftpConnectionService.GetSFTPConnectionAsync(sftpconnectionid, organizationid, userId);
            if (connection.Status == MutationStatus.NotFound || connection.Data is null)
            {
                return NotFound("SFTP connection not found.");
            }
            if (connection.Status == MutationStatus.Forbidden)
            {
                return Forbid("You do not have permission to access this SFTP connection.");
            }

            SFTPActionRenameResult renameResult = await _sftpActionService.RenameFileAsync(connection.Data, renameRequest.OldRemoteFilePath, renameRequest.NewRemoteFilePath, HttpContext.RequestAborted);

            return Ok(renameResult.ToResponse());
        }

        [HttpDelete("delete")]
        [ProducesResponseType(typeof(List<SFTPActionDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Permission(PermissionFlags.SftpActionDelete)]
        public async Task<IActionResult> DeleteFileAsync([FromRoute] int organizationid, [FromRoute] int sftpconnectionid, [FromQuery] string remoteFilePath)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            var connection = await _sftpConnectionService.GetSFTPConnectionAsync(sftpconnectionid, organizationid, userId);
            if (connection.Status == MutationStatus.NotFound || connection.Data is null)
            {
                return NotFound("SFTP connection not found.");
            }
            if (connection.Status == MutationStatus.Forbidden)
            {
                return Forbid("You do not have permission to access this SFTP connection.");
            }

            List<SFTPActionDeleteResult> deleteResult = await _sftpActionService.DeleteAsync(connection.Data, remoteFilePath, HttpContext.RequestAborted);

            return Ok(deleteResult.Select(result => result.ToResponse()));
        }
    }
}
