using Controller.DTO.SFTPConnection;
using Controller.Extensions;
using Domain.DTO;
using Domain.Enum;
using Domain.Interface;
using Infrastructure.Models;
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
    }
}
