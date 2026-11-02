using Controller.DTO.response;
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
    public class SFTPConnectionController(UserManager<User> userManager, ISFTPConnectionService sFTPConnectionService) : ControllerBase
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly ISFTPConnectionService _sFTPConnectionService = sFTPConnectionService;

        [HttpPost("requestCode")]
        [Authorize]
        public async Task<ActionResult<SFTPConnectionAuthCodeDTO>> RequestCode(string organizationId)
        {
            if (HttpContext is null || User is null)
            {
                return Unauthorized();
            }

            if (organizationId is null)
            {
                return BadRequest("Organization ID is required.");
            }

            // TODO: Check if they're allowed to generate auth codes?
            // Both based on permissions and if they're in the organization?
            User? user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                return Unauthorized();
            }

            SFTPConnectionAuthCode authCode = await _sFTPConnectionService.GenerateVerificationCodeAsync(int.Parse(organizationId), user.Id);
            return authCode.ToDTO();
        }
    }
}
