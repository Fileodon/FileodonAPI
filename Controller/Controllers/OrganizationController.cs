using Controller.Controllers.Authorization;
using Domain.Interface;
using Domain.Service;
using Infrastructure;
using Infrastructure.Models;
using Infrastructure.Models.DTO;
using Infrastructure.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Controller.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrganizationController : ControllerBase
{
    private readonly IOrganizationService _organizationService;
    private readonly IAuthorizationService _authorizationService;
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IUserOrganizationService _userOrgService;

    public OrganizationController(
        IOrganizationService organizationService,
        IAuthorizationService authorizationService,
        UserManager<User> userManager,
        ApplicationDbContext context,
        IUserOrganizationService userOrgService)
    {
        _organizationService = organizationService;
        _authorizationService = authorizationService;
        _userManager = userManager;
        _context = context;
        _userOrgService = userOrgService;
    }


    private async Task<bool> HasPermissionAsync(
    int organizationId,
    PermissionFlags permission)
    {
        var result = await _authorizationService.AuthorizeAsync(
            User,
            organizationId,
            new PermissionRequirement(permission));

        return result.Succeeded;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrganizations()
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var organizations = await _context.UserOrganizations
            .AsNoTracking()
            .Where(userOrg =>
                userOrg.UserId == userId &&
                userOrg.Organization != null)
            .Select(userOrg => new
            {
                Id = userOrg.Organization!.Id,
                OrgName = userOrg.Organization!.OrgName
            })
            .Distinct()
            .ToListAsync();

        return Ok(organizations);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrganizationById(int id)
    {
        if(!await HasPermissionAsync(id, PermissionFlags.Read))
        {
            return Forbid();
        }

        var organization = await _organizationService.GetByIdAsync(id);


        if (organization is null)
            return NotFound();

        return Ok(organization);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] OrganizationDTO dto)
    {
        if(!await HasPermissionAsync(0, PermissionFlags.Create))
        {
            return Forbid();
        }

        var organization = new Organization
        {
            OrgName = dto.Org_Name
        };

        var result = await _organizationService.PostAsync(organization);

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateOrganization(int id, [FromBody] OrganizationDTO organization)
    {
        if (id != organization.Id)
            return BadRequest();

        if(!await HasPermissionAsync(id, PermissionFlags.Update))
        {
            return Forbid();
        }

        var existing = await _organizationService.GetByIdAsync(id);

        if (existing is null)
            return NotFound();

        existing.OrgName = organization.Org_Name;

        var updated = await _organizationService.UpdateAsync(existing);

        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteOrganization(int id)
    {
        var authorization = await _authorizationService.AuthorizeAsync(
            User,
            id,
            new PermissionRequirement(PermissionFlags.Delete));

        if (!authorization.Succeeded)
            return Forbid();

        var deleted = await _organizationService.DeleteAsync(id);

        if (deleted is null)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{organizationId:int}/members")]
    public async Task<IActionResult> AddMember(
    int organizationId,
    [FromBody] UserOrgDTO dto)
    {
        // Permission på bruger ift. at tilføje medelemmer til organisationen
        if (!await HasPermissionAsync(organizationId, PermissionFlags.Create))
            return Forbid();

        if (string.IsNullOrWhiteSpace(dto.UserId) ||
            string.IsNullOrWhiteSpace(dto.RoleId))
            return BadRequest("UserId og RoleId skal udfyldes.");

        var user = await _userManager.FindByIdAsync(dto.UserId);

        if (user is null)
            return NotFound("Brugeren findes ikke."); //status 404

        var role = await _context.AspNetRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.Id == dto.RoleId &&
                r.OrganizationId == organizationId);

        if (role is null)
            return BadRequest("Rollen tilhører ikke organisationen."); //status 400

        var alreadyMember = await _context.UserOrganizations
            .AnyAsync(m =>
                m.UserId == dto.UserId &&
                m.OrganizationId == organizationId);

        if (alreadyMember)
            return Conflict("Brugeren er allerede medlem."); //status 409

        var membership = await _userOrgService.PostAsync(
            new UserOrganization
            {
                UserId = dto.UserId,
                OrganizationId = organizationId,
                RoleId = dto.RoleId
            });

        return Ok(new
        {
            membership.Id,
            membership.UserId,
            membership.OrganizationId,
            membership.RoleId
        });
    }

}
