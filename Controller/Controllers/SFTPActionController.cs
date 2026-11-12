using Controller.DTO.SFTPAction;
using Controller.Extensions;
using Domain.Enum;
using Domain.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers
{
    [Route("organization/{organizationid}/[controller]/{sftpconnectionid}")]
    [ApiController]
    [Authorize]
    public class SFTPActionController : ControllerBase
    {
        private readonly ISFTPConnectionService _sftpConnectionService;
        private readonly ISFTPActionService _sftpActionService;

        public SFTPActionController(ISFTPConnectionService sftpConnectionService, ISFTPActionService sftpActionService)
        {
            _sftpConnectionService = sftpConnectionService;
            _sftpActionService = sftpActionService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824, ValueCountLimit = 1073741824)] // 1 GB limit, adjust as needed
        [ProducesResponseType(StatusCodes.Status200OK)]
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

            if (!uploadRequest.OverrideExisting &&
                await _sftpActionService.FileExistsAsync(connection.Data, uploadRequest.RemoteFilePath))
            {
                return Conflict("File already exists on the SFTP server and override is disabled.");
            }

            using var fileStream = uploadRequest.File.OpenReadStream();
            bool uploadSuccess = await _sftpActionService.UploadFileAsync(connection.Data, fileStream, uploadRequest.RemoteFilePath);

            if (uploadSuccess)
            {
                return Ok("File uploaded successfully.");
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to upload the file.");
            }
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
