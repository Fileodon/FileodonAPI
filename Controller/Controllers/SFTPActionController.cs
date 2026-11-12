using Controller.DTO.SFTPAction;
using Controller.Extensions;
using Domain.Enum;
using Domain.Interface;
using Domain.Worker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet.Sftp;

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
                    if (await _sftpActionService.FileExistsAsync(connection.Data, remoteFilePath))
                    {
                        return Conflict("A file already exists on the SFTP server and override is disabled.");
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

        [HttpGet("list")]
        [ProducesResponseType(typeof(IEnumerable<SFTPActionListFilesResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ListFilesAsync([FromRoute] int organizationid, [FromRoute] int sftpconnectionid, [FromBody] SFTPActionListFilesRequest listRequest)
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

            IEnumerable<ISftpFile> response = await _sftpActionService.ListFilesAsync(connection.Data, listRequest.RemoteDirectoryPath, HttpContext.RequestAborted);

            // Skip "."
            var files = response
                .Where(file => file.Name != ".")
                .Select(SFTPActionListFilesResponseExtensions.FromSftpFile)
                .ToList();

            return Ok(files);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

            bool fileExists = await _sftpActionService.FileExistsAsync(connection.Data, remoteFilePath);
            return Ok(new { FileExists = fileExists });
        }
    }
}
