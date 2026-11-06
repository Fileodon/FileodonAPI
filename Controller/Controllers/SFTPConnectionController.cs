using Controller.DTO.SFTPConnection;
using Controller.Extensions;
using Domain.DTO;
using Domain.Enum;
using Domain.Interface;
using Infrastructure.Models;
using Infrastructure.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers
{
    [Route("api/organization/{organizationId}/[controller]")]
    [Authorize]
    [ApiController]
    public class SFTPConnectionController(ISFTPConnectionService sFTPConnectionService) : ControllerBase
    {
        private readonly ISFTPConnectionService _sFTPConnectionService = sFTPConnectionService;

        [HttpGet("{sFTPConnectionId}")]
        [ProducesResponseType(typeof(BaseSFTPConnectionGetResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSFTPConnection(int organizationId, int sFTPConnectionId)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationResult<SFTPConnection> result = await _sFTPConnectionService.GetSFTPConnectionAsync(sFTPConnectionId, organizationId, userId);

            if (result.Status == MutationStatus.NotFound || result.Data is null)
            {
                return NotFound();
            }
            if (result.Status == MutationStatus.Forbidden)
            {
                return Forbid();
            }
            return Ok(result.Data.ToBaseGetResponseDTO());
        }

        [HttpDelete("{sFTPConnectionId}")]
        [ProducesResponseType(typeof(SFTPConnectionDeleteResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteSFTPConnection(int organizationId, int sFTPConnectionId)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationResult<SFTPConnection> result = await _sFTPConnectionService.DeleteSFTPConnectionAsync(sFTPConnectionId, organizationId, userId);

            if (result.Status == MutationStatus.NotFound || result.Data is null)
            {
                return NotFound();
            }
            if (result.Status == MutationStatus.Forbidden)
            {
                return Forbid();
            }
            return Ok(result.Data.ToSFTPConnectionDeleteResponse());
        }

        [HttpPatch("{sFTPConnectionId}")]
        [ProducesResponseType(typeof(SFTPConnectionUpdateResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateSFTPConnection(int organizationId, int sFTPConnectionId, [FromBody] SFTPConnectionUpdateRequest request)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationResult<SFTPConnection> result = await _sFTPConnectionService.UpdateSFTPConnectionAsync(sFTPConnectionId, organizationId, userId, request.DisplayName);

            if (result.Status == MutationStatus.NotFound || result.Data is null)
            {
                return NotFound();
            }
            if (result.Status == MutationStatus.Forbidden)
            {
                return Forbid();
            }
            return Ok(new SFTPConnectionUpdateResponse(result.Data.DisplayName));
        }

        [HttpGet("{sFTPConnectionId}/detailed")]
        [ProducesResponseType(typeof(DetailedSFTPConnectionGetResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetDetailedSFTPConnection(int organizationId, int sFTPConnectionId)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationResult<SFTPConnection> result = await _sFTPConnectionService.GetSFTPConnectionAsync(sFTPConnectionId, organizationId, userId);

            if (result.Status == MutationStatus.NotFound || result.Data is null)
            {
                return NotFound();
            }
            if (result.Status == MutationStatus.Forbidden)
            {
                return Forbid();
            }
            return Ok(result.Data.ToDetailedGetResponseDTO());
        }

        [HttpGet("{sFTPConnectionId}/poll")]
        [ProducesResponseType(typeof(SFTPConnectionStatus), 200, Description = "Rejected")]
        [ProducesResponseType(typeof(SFTPConnectionStatus), 202, Description = "Pending")]
        [ProducesResponseType(typeof(SFTPConnectionStatus), 303, Description = "Accepted")]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PollStatus(int organizationId, int sFTPConnectionId)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationResult<SFTPConnection> result = await _sFTPConnectionService.PollSFTPConnectionStatusAsync(sFTPConnectionId, organizationId, userId);

            if (result.Status == MutationStatus.NotFound || result.Data is null)
            {
                return NotFound();
            }
            if (result.Status == MutationStatus.Forbidden)
            {
                return Forbid();
            }

            var status = result.Data.Status;
            switch (status)
            {
                case SFTPConnectionStatus.Rejected:
                    return Ok(status);
                case SFTPConnectionStatus.Pending:
                    Response.Headers.RetryAfter = "5"; // Suggests the client to retry after 5 seconds
                    return Accepted(status);
                case SFTPConnectionStatus.Accepted:
                    return this.SeeOther(
                        $"/api/organization/{result.Data.OrganizationId}/sftpconnection/{result.Data.Id}/detailed",
                        status.ToString());
                default:
                    return StatusCode(500, "Unknown status.");
            }
        }

        [HttpPost("requestCode")]
        [ProducesResponseType(typeof(SFTPConnectionAuthCodeDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RequestCode(int organizationId)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationResult<SFTPConnectionAuthCode> result = await _sFTPConnectionService.GenerateVerificationCodeAsync(organizationId, userId);

            if (result.Status == MutationStatus.NotFound || result.Data is null)
            {
                return NotFound();
            }
            if (result.Status == MutationStatus.Forbidden)
            {
                return Forbid();
            }
            return Ok(result.Data.ToDTO());
        }

        [HttpPost]
        [ProducesResponseType(typeof(SFTPConnectionCreateResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [AllowAnonymous]
        public async Task<IActionResult> AddServer(int organizationId, [FromBody] SFTPConnectionCreateRequest request)
        {
            MutationResult<SFTPConnection> result = await _sFTPConnectionService.AddSFTPConnectionAsync(request.AuthCode, organizationId, request.DisplayName, request.Host, request.Port);

            if (result.Status == MutationStatus.NotFound || result.Data is null)
            {
                return NotFound();
            }
            if (result.Status == MutationStatus.Forbidden)
            {
                return Forbid();
            }
            return Ok(result.Data.ToDTO());
        }

        [HttpPatch("status")]
        [ProducesResponseType(typeof(SFTPConnectionStatusUpdateResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateStatus(int organizationId, [FromBody] SFTPConnectionStatusUpdateRequest request)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationResult<SFTPConnection> result = await _sFTPConnectionService.UpdateSFTPConnectionStatusAsync(request.SFTPConnectionId, organizationId, userId, request.NewStatus);

            if (result.Status == MutationStatus.NotFound || result.Data is null)
            {
                return NotFound();
            }
            if (result.Status == MutationStatus.Forbidden)
            {
                return Forbid();
            }
            return Ok(result.Data.ToStatusUpdateResponse());
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<BaseSFTPConnectionGetResponse>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetOrganizationSFTPConnections(int organizationId)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            IEnumerable<SFTPConnection> result = await _sFTPConnectionService.GetOrganizationSFTPConnectionsAsync(organizationId, userId);

            return Ok(result.Select(conn => conn.ToBaseGetResponseDTO()));
        }

        [HttpGet("all/detailed")]
        [ProducesResponseType(typeof(IEnumerable<DetailedSFTPConnectionGetResponse>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetDetailedOrganizationSFTPConnections(int organizationId)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            IEnumerable<SFTPConnection> result = await _sFTPConnectionService.GetOrganizationSFTPConnectionsAsync(organizationId, userId);

            return Ok(result.Select(conn => conn.ToDetailedGetResponseDTO()));
        }
    }
}
