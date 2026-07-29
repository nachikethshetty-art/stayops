using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Common;
using StayOps.Application.NightAudit;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/v1/hotels/{hotelId:guid}/night-audit")]
[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager},{Roles.FinanceUser}")]
public class NightAuditController(INightAuditService service) : ControllerBase
{
    private Guid? CurrentUserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    [HttpPost("run")]
    public async Task<ActionResult<NightAuditRunDto>> Run(Guid hotelId, CancellationToken ct)
        => Ok(await service.RunAsync(hotelId, CurrentUserId, ct));

    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<NightAuditRunDto>>> GetHistory(Guid hotelId, CancellationToken ct)
        => Ok(await service.GetHistoryAsync(hotelId, ct));

    [HttpGet("runs/{runId:guid}/exceptions")]
    public async Task<ActionResult<IReadOnlyList<NightAuditExceptionDto>>> GetExceptions(Guid hotelId, Guid runId, CancellationToken ct)
        => Ok(await service.GetExceptionsAsync(runId, ct));
}
