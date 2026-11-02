using Controller.DTO.Organization;
using Domain.DTO;
using Domain.Enum;
using Domain.Interface;
using Infrastructure.Models;
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
        [ProducesResponseType(typeof(GetOrganizationRequest), 200)]
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
            return Ok(organization.ToGetOrganizationRequest());
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

            MutationResult<Organization> result = await _organizationService.DeleteOrganizationByIdAsync(organizationId, userId);
            if (result.Status == MutationStatus.NotFound || result.Data is null)
            {
                return NotFound();
            }
            if (result.Status == MutationStatus.Forbidden)
            {
                return Forbid();
            }
            return Ok(result.Data.OrgName);
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
            if (result.Status == MutationStatus.NotFound || result.Data is null)
            {
                return NotFound();
            }
            if (result.Status == MutationStatus.Forbidden)
            {
                return Forbid();
            }
            return Ok(result.Data.OrgName);
        }
    }
}
