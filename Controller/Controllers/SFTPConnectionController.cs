using Controller.DTO.SFTPConnection;
using Controller.Extensions;
using Controller.Middleware.Attribute;
using Domain.DTO;
using Domain.Enum;
using Domain.Interface;
using Infrastructure.Models;
using Infrastructure.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers
{
    [Route("api/organization/{organizationid}/[controller]")]
    [Authorize]
    [ApiController]
    [RequireVerifiedEmail]
    public class SFTPConnectionController : ControllerBase
    {
        private readonly ISFTPConnectionService _sFTPConnectionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SFTPConnectionController(ISFTPConnectionService sFTPConnectionService, IHttpContextAccessor httpContextAccessor)
        {
            _sFTPConnectionService = sFTPConnectionService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("{sftpconnectionid}")]
        [ProducesResponseType(typeof(BaseSFTPConnectionGetResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [AllowUnverifiedEmail]
        public async Task<IActionResult> GetSFTPConnection(int organizationid, int sftpconnectionid)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationEnvelope<SFTPConnection> result = await _sFTPConnectionService.GetSFTPConnectionAsync(sftpconnectionid, organizationid, userId);

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

        [HttpDelete("{sftpconnectionid}")]
        [ProducesResponseType(typeof(SFTPConnectionDeleteResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteSFTPConnection(int organizationid, int sftpconnectionid)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationEnvelope<SFTPConnection> result = await _sFTPConnectionService.DeleteSFTPConnectionAsync(sftpconnectionid, organizationid, userId);

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

        [HttpPatch("{sftpconnectionid}")]
        [ProducesResponseType(typeof(SFTPConnectionUpdateResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateSFTPConnection(int organizationid, int sftpconnectionid, [FromBody] SFTPConnectionUpdateRequest request)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationEnvelope<SFTPConnection> result = await _sFTPConnectionService.UpdateSFTPConnectionAsync(sftpconnectionid, organizationid, userId, request.DisplayName, request.Port, request.IntermediaryPort);

            if (result.Status == MutationStatus.NotFound || result.Data is null)
            {
                return NotFound();
            }
            if (result.Status == MutationStatus.Forbidden)
            {
                return Forbid();
            }
            return Ok(result.Data.ToSFTPConnectionUpdateResponse());
        }

        [HttpGet("{sftpconnectionid}/detailed")]
        [ProducesResponseType(typeof(DetailedSFTPConnectionGetResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetDetailedSFTPConnection(int organizationid, int sftpconnectionid)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationEnvelope<SFTPConnection> result = await _sFTPConnectionService.GetSFTPConnectionAsync(sftpconnectionid, organizationid, userId);

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

        [AllowAnonymous]
        [HttpGet("{sftpconnectionid}/poll")]
        [ProducesResponseType(typeof(SFTPConnectionStatus), 200, Description = "Rejected")]
        [ProducesResponseType(typeof(SFTPConnectionPollResponse), 200, Description = "Accepted and returns publicKey")]
        [ProducesResponseType(typeof(SFTPConnectionStatus), 202, Description = "Pending")]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PollStatus(int organizationid, int sftpconnectionid)
        {
            MutationEnvelope<SFTPConnection> result = await _sFTPConnectionService.PollSFTPConnectionStatusAsync(sftpconnectionid, organizationid);

            if (result.Status == MutationStatus.NotFound || result.Data is null)
            {
                return NotFound();
            }
            if (result.Status == MutationStatus.Forbidden)
            {
                return Forbid();
            }

            // TODO: hvis pending

            var status = result.Data.Status;
            switch (status)
            {
                case SFTPConnectionStatus.Rejected:
                    return Ok(result.Data.toPollResponse());
                case SFTPConnectionStatus.Pending:
                    Response.Headers.RetryAfter = "5"; // Suggests the client to retry after 5 seconds
                    return Accepted(result.Data.toPollResponse());
                case SFTPConnectionStatus.Accepted:
                    return Ok(result.Data.toPollResponse());
                default:
                    return StatusCode(500, "Unknown status.");
            }
        }

        [HttpPost("requestCode")]
        [ProducesResponseType(typeof(SFTPConnectionAuthCodeResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RequestCode(int organizationid)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationEnvelope<SFTPConnectionAuthCode> result = await _sFTPConnectionService.GenerateVerificationCodeAsync(organizationid, userId);

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

        // Fjerner {orgid} fra url
        [HttpPost("~/api/organization/[controller]")]
        [ProducesResponseType(typeof(SFTPConnectionCreateResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [AllowAnonymous]
        public async Task<IActionResult> AddServer([FromBody] SFTPConnectionCreateRequest request)
        {
            MutationEnvelope<SFTPConnectionAuthCode> AuthCode = await _sFTPConnectionService.GetSFTPConnectionAuthCodeAsync(request.AuthCode);
            if (AuthCode.Status == MutationStatus.NotFound || AuthCode.Data is null)
            {
                return NotFound();
            }
            if (AuthCode.Data.ExpiresAt < DateTime.UtcNow)
            {
                return Forbid("Auth code has expired.");
            }

            HttpContext.User.AddIdentity(new System.Security.Claims.ClaimsIdentity(
                new[] { new System.Security.Claims.Claim("id", AuthCode.Data.UserId.ToString()) }));

            MutationEnvelope<SFTPConnection> result = await _sFTPConnectionService.AddSFTPConnectionAsync(AuthCode.Data.VerificationCode, AuthCode.Data.OrganizationId, request.DisplayName, request.Host, request.Port, request.IntermediaryPort);

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
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateStatus(int organizationid, [FromBody] SFTPConnectionStatusUpdateRequest request)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationEnvelope<SFTPConnection> result = await _sFTPConnectionService.UpdateSFTPConnectionStatusAsync(request.SFTPConnectionId, organizationid, userId, request.NewStatus);

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
        [AllowUnverifiedEmail]
        public async Task<IActionResult> GetOrganizationSFTPConnections(int organizationid)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            IEnumerable<SFTPConnection> result = await _sFTPConnectionService.GetOrganizationSFTPConnectionsAsync(organizationid, userId);

            return Ok(result.Select(conn => conn.ToBaseGetResponseDTO()));
        }

        [HttpGet("all/detailed")]
        [ProducesResponseType(typeof(IEnumerable<DetailedSFTPConnectionGetResponse>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetDetailedOrganizationSFTPConnections(int organizationid)
        {
            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            IEnumerable<SFTPConnection> result = await _sFTPConnectionService.GetOrganizationSFTPConnectionsAsync(organizationid, userId);

            return Ok(result.Select(conn => conn.ToDetailedGetResponseDTO()));
        }

        [HttpGet("~/api/organization/[controller]/pam/publickey")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [AllowAnonymous]
        public async Task<IActionResult> GetPAMPublicKey()
        {
            string? publicKey = await _sFTPConnectionService.GetPAMPublicKeyAsync();

            if (publicKey is null)
            {
                return NotFound();
            }

            return Ok(publicKey);
        }
    }
}
