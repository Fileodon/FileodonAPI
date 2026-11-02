using Controller.DTO.response;
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
    [ApiController]
    public class SFTPConnectionController(ISFTPConnectionService sFTPConnectionService) : ControllerBase
    {
        private readonly ISFTPConnectionService _sFTPConnectionService = sFTPConnectionService;

        [HttpPost("requestCode")]
        [Authorize]
        public async Task<ActionResult<SFTPConnectionAuthCodeDTO>> RequestCode(string organizationId)
        {
            if (organizationId is null)
            {
                return BadRequest("Organization ID is required.");
            }

            if (!User.TryGetClaimValue("id", out var userId) || userId is null)
            {
                return Unauthorized("User ID claim is missing.");
            }

            MutationResult<SFTPConnectionAuthCode> result = await _sFTPConnectionService.GenerateVerificationCodeAsync(int.Parse(organizationId), userId);

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
