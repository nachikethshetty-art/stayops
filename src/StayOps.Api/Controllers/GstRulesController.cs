using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Common;
using StayOps.Application.GstRules;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/v1/hotels/{hotelId:guid}/gst-rules")]
[Authorize]
public class GstRulesController(IGstRuleService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GstRuleDto>>> GetAll(Guid hotelId, CancellationToken ct)
        => Ok(await service.GetForHotelAsync(hotelId, ct));

    [HttpPost]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.FinanceUser}")]
    public async Task<ActionResult<GstRuleDto>> Create(Guid hotelId, [FromBody] CreateGstRuleRequest request, CancellationToken ct)
    {
        var allowGlobal = User.IsInRole(Roles.SuperAdmin);
        return Ok(await service.CreateAsync(hotelId, request, allowGlobal, ct));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.FinanceUser}")]
    public async Task<ActionResult<GstRuleDto>> Update(Guid hotelId, Guid id, [FromBody] UpdateGstRuleRequest request, CancellationToken ct)
        => Ok(await service.UpdateAsync(id, request, ct));
}
