using System.Security.Claims;
using Controller.DTO.response;
using Controller.Extensions;
using Controller.Utility;
using Domain.Interface;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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

            // TODO: Check if they're allowed to generate auth codes?
            // Both based on permissions and if they're in the organization?
            SFTPConnectionAuthCode authCode = await _sFTPConnectionService.GenerateVerificationCodeAsync(int.Parse(organizationId), userId);
            return authCode.ToDTO();
        }
    }
}
