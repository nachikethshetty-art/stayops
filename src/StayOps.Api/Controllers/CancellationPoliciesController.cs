using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.CancellationPolicies;
using StayOps.Application.Common;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/v1/hotels/{hotelId:guid}/cancellation-policies")]
[Authorize]
public class CancellationPoliciesController(ICancellationPolicyService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CancellationPolicyDto>>> GetAll(Guid hotelId, CancellationToken ct)
        => Ok(await service.GetByHotelAsync(hotelId, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CancellationPolicyDto>> GetById(Guid hotelId, Guid id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(hotelId, id, ct));

    [HttpPost]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager}")]
    public async Task<ActionResult<CancellationPolicyDto>> Create(Guid hotelId, [FromBody] CreateCancellationPolicyRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(hotelId, request, ct);
        return CreatedAtAction(nameof(GetById), new { hotelId, id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager}")]
    public async Task<ActionResult<CancellationPolicyDto>> Update(Guid hotelId, Guid id, [FromBody] UpdateCancellationPolicyRequest request, CancellationToken ct)
        => Ok(await service.UpdateAsync(hotelId, id, request, ct));

    [HttpPost("{policyId:guid}/rules")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager}")]
    public async Task<ActionResult<CancellationPolicyDto>> AddRule(Guid hotelId, Guid policyId, [FromBody] UpsertCancellationPolicyRuleRequest request, CancellationToken ct)
        => Ok(await service.AddRuleAsync(hotelId, policyId, request, ct));

    [HttpPut("{policyId:guid}/rules/{ruleId:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager}")]
    public async Task<ActionResult<CancellationPolicyDto>> UpdateRule(Guid hotelId, Guid policyId, Guid ruleId, [FromBody] UpsertCancellationPolicyRuleRequest request, CancellationToken ct)
        => Ok(await service.UpdateRuleAsync(hotelId, policyId, ruleId, request, ct));

    [HttpDelete("{policyId:guid}/rules/{ruleId:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager}")]
    public async Task<IActionResult> DeleteRule(Guid hotelId, Guid policyId, Guid ruleId, CancellationToken ct)
    {
        await service.DeleteRuleAsync(hotelId, policyId, ruleId, ct);
        return NoContent();
    }
}
