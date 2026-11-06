using Controller.DTO.AuditLog;
using Domain.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AuditLogController: ControllerBase
{
    public AuditLogController(){
    }

    [HttpGet]
    [ProducesResponseType(typeof(GetAllAuditLogsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAuditLogs([FromQuery] string filter)
    {
        // Authorize - Måske det skulle authorizes pr resource, altså som man altid kan blive vist audit logs på de resourcer man har read på.
        GetAllAuditLogsResponse dto = new GetAllAuditLogsResponse
        {
            test = filter
        };
        return Ok(dto);
    }
}