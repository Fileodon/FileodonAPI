using Controller.DTO.Organization;
using Domain.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController(IOrganizationService organizationService) : ControllerBase
    {
        private readonly IOrganizationService _organizationService = organizationService;

        [HttpGet("{organizationId}")]
        [Authorize]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetOrganizationById(int organizationId)
        {
            string? userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var organization = await _organizationService.GetOrganizationByIdAsync(organizationId, userId);
            if (organization == null)
            {
                return NotFound();
            }
            return Ok(organization.OrgName);
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(string), 201)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request)
        {
            string? userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var createdOrganization = await _organizationService.CreateOrganizationAsync(request.OrgName, request.WorkspaceURL, request.ContactEmail, userId);
            return Created($"/api/organization/{createdOrganization.Id}", createdOrganization.OrgName);
        }

        [HttpDelete("{organizationId}")]
        [Authorize]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteOrganization(int organizationId)
        {
            string? userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _organizationService.DeleteOrganizationByIdAsync(organizationId, userId);
            if (result.Status == OrganizationMutationStatus.NotFound)
            {
                return NotFound();
            }
            if (result.Status == OrganizationMutationStatus.Forbidden)
            {
                return Forbid();
            }
            return Ok(result.Organization!.OrgName);
        }

        [HttpPut("{organizationId}")]
        [Authorize]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateOrganization(int organizationId, [FromBody] string newOrganizationName)
        {
            string? userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _organizationService.UpdateOrganizationAsync(organizationId, newOrganizationName, userId);
            if (result.Status == OrganizationMutationStatus.NotFound)
            {
                return NotFound();
            }
            if (result.Status == OrganizationMutationStatus.Forbidden)
            {
                return Forbid();
            }
            return Ok(result.Organization!.OrgName);
        }
    }
}
