using Controller.DTO.AuditLog;
using Domain.Interface;
using Domain.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AuditLogController(IAuditLogService service): ControllerBase
{
    private readonly IAuditLogService _service = service;
    [HttpGet("{organizationId}")]
    [ProducesResponseType(typeof(GetAllAuditLogsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAuditLogs(int organizationId, [FromQuery] string? filter)
    {
        // Authorize - Måske det skulle authorizes pr resource, altså som man altid kan blive vist audit logs på de resourcer man har read på.
        var data = await _service.GetAllAuditLogsInOrganization(organizationId);

        GetAllAuditLogsResponse dto = new GetAllAuditLogsResponse(
            data.organizationAuditLogs,
            data.SFTPConnectionAuditLogs,
            data.UserOrganizationAuditLogs,
            data.RoleAuditLogs
        );
        return Ok(dto);
    }
}