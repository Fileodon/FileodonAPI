using Domain.Interface;
using Infrastructure.Models;
using Infrastructure.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        public OrganizationController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrganizations()
        {
            var organizations = await _organizationService.GetAllAsync();
            return Ok(organizations);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrganizationById(int id)
        {
            var organization = await _organizationService.GetByIdAsync(id);

            if (organization is null)
                return NotFound();

            return Ok(organization);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OrganizationDTO dto)
        {
            var organization = new Organization
            {
                OrgName = dto.Org_Name
            };

            var result = await _organizationService.PostAsync(organization);

            return Ok(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateOrganization(
            int id,
            [FromBody] OrganizationDTO organization)
        {
            if (id != organization.Id)
                return BadRequest();

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
            var deleted = await _organizationService.DeleteAsync(id);

            if (deleted is null)
                return NotFound();

            return NoContent();
        }

    }
}
