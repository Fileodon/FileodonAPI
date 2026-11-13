using Controller.DTO.Organization;
using Controller.Middleware.Attribute;
using Domain.DTO;
using Domain.Enum;
using Domain.Interface;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers;

[Authorize]
[Route("[controller]")]
[ApiController]
public class OrganizationController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet("{organizationId}")]
    // PErmissions her. skal måske rettere afkorte responsen? Ellers skal vi bare ikke permissions på dette
    [ProducesResponseType(typeof(GetOrganizationResponse), 200)]
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
        return Ok(organization.ToGetOrganizationResponse());
    }

    [HttpPost]
    [ProducesResponseType(typeof(GetOrganizationResponse), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request)
    {
        string? userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var createdOrganization = await _organizationService.CreateOrganizationAsync(request.OrgName, request.WorkspaceURL, request.ContactEmail, userId);
        return Created($"/api/organization/{createdOrganization.Id}", createdOrganization.ToGetOrganizationResponse());
    }

    [HttpDelete("{organizationId}")]
    [Permission(Infrastructure.Models.Enums.PermissionFlags.UserOrganizationDelete)]
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

        MutationEnvelope<Organization> result = await _organizationService.DeleteOrganizationByIdAsync(organizationId, userId);
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
    [Permission(Infrastructure.Models.Enums.PermissionFlags.OrganizationUpdate)]
    [ProducesResponseType(typeof(GetOrganizationResponse), 200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateOrganization(int organizationId, 
        [FromBody] EditOrganizationRequest validatedRequest)
    {
        string? userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _organizationService.UpdateOrganizationAsync(organizationId, validatedRequest.OrgName, validatedRequest.WorkspaceURL, validatedRequest.ContactEmail, userId);
        if (result.Status == MutationStatus.NotFound || result.Data is null)
        {
            return NotFound();
        }
        if (result.Status == MutationStatus.Forbidden)
        {
            return Forbid();
        }
        return Ok(result.Data.ToGetOrganizationResponse());
    }
}