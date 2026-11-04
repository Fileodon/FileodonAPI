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
    [Route("api/organization/[controller]")]
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
        public async Task<IActionResult> GetSFTPConnection(int sFTPConnectionId)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationResult<SFTPConnection> result = await _sFTPConnectionService.GetSFTPConnectionAsync(sFTPConnectionId, userId);

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

        [HttpGet("detailed/{sFTPConnectionId}")]
        [ProducesResponseType(typeof(DetailedSFTPConnectionGetResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetDetailedSFTPConnection(int sFTPConnectionId)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationResult<SFTPConnection> result = await _sFTPConnectionService.GetSFTPConnectionAsync(sFTPConnectionId, userId);

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

        [HttpPost("requestCode")]
        [ProducesResponseType(typeof(SFTPConnectionAuthCodeDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RequestCode([FromForm] int organizationId)
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
        public async Task<IActionResult> AddServer([FromBody] SFTPConnectionCreateRequest request)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationResult<SFTPConnection> result = await _sFTPConnectionService.AddSFTPConnectionAsync(request.AuthCode, userId, request.DisplayName, request.Host, request.Port);

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
        public async Task<IActionResult> UpdateStatus([FromBody] SFTPConnectionStatusUpdateRequest request)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationResult<SFTPConnection> result = await _sFTPConnectionService.UpdateSFTPConnectionStatusAsync(request.SFTPConnectionId, userId, request.NewStatus);

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

        [HttpGet("poll")]
        [ProducesResponseType(typeof(SFTPConnectionStatus), 200)] // Rejected
        [ProducesResponseType(typeof(SFTPConnectionStatus), 202)] // Pending
        [ProducesResponseType(typeof(SFTPConnectionStatus), 303)] // Accepted
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PollStatus([FromBody] int sFTPConnectionId)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationResult<SFTPConnectionStatus> result = await _sFTPConnectionService.PollSFTPConnectionStatusAsync(sFTPConnectionId, userId);

            if (result.Status == MutationStatus.NotFound)
            {
                return NotFound();
            }
            if (result.Status == MutationStatus.Forbidden)
            {
                return Forbid();
            }

            var status = result.Data;
            switch (status)
            {
                case SFTPConnectionStatus.Rejected:
                    return Ok(status);
                case SFTPConnectionStatus.Pending:
                    Response.Headers.RetryAfter = "5"; // Suggests the client to retry after 5 seconds
                    return Accepted(status);
                case SFTPConnectionStatus.Accepted:
                    return this.SeeOther($"/api/organization/SFTPConnection/");
                default:
                    return StatusCode(500, "Unknown status.");
            }
        }
    }
}
