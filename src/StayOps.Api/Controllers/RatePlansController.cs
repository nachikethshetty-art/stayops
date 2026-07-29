using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Common;
using StayOps.Application.Rates;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/v1/hotels/{hotelId:guid}/rate-plans")]
[Authorize]
public class RatePlansController(IRatePlanService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RatePlanDto>>> GetAll(Guid hotelId, CancellationToken ct)
        => Ok(await service.GetByHotelAsync(hotelId, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RatePlanDto>> GetById(Guid hotelId, Guid id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(hotelId, id, ct));

    [HttpPost]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager}")]
    public async Task<ActionResult<RatePlanDto>> Create(Guid hotelId, [FromBody] CreateRatePlanRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(hotelId, request, ct);
        return CreatedAtAction(nameof(GetById), new { hotelId, id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager}")]
    public async Task<ActionResult<RatePlanDto>> Update(Guid hotelId, Guid id, [FromBody] UpdateRatePlanRequest request, CancellationToken ct)
        => Ok(await service.UpdateAsync(hotelId, id, request, ct));

    [HttpGet("{ratePlanId:guid}/prices")]
    public async Task<ActionResult<IReadOnlyList<RatePlanPriceDto>>> GetPrices(Guid hotelId, Guid ratePlanId, CancellationToken ct)
        => Ok(await service.GetPricesAsync(hotelId, ratePlanId, ct));

    [HttpPost("{ratePlanId:guid}/prices")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager}")]
    public async Task<ActionResult<RatePlanPriceDto>> AddPrice(Guid hotelId, Guid ratePlanId, [FromBody] CreateRatePlanPriceRequest request, CancellationToken ct)
        => Ok(await service.AddPriceAsync(hotelId, ratePlanId, request, ct));

    [HttpPut("{ratePlanId:guid}/prices/{priceId:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager}")]
    public async Task<ActionResult<RatePlanPriceDto>> UpdatePrice(Guid hotelId, Guid ratePlanId, Guid priceId, [FromBody] UpdateRatePlanPriceRequest request, CancellationToken ct)
        => Ok(await service.UpdatePriceAsync(hotelId, ratePlanId, priceId, request, ct));

    [HttpDelete("{ratePlanId:guid}/prices/{priceId:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager}")]
    public async Task<IActionResult> DeletePrice(Guid hotelId, Guid ratePlanId, Guid priceId, CancellationToken ct)
    {
        await service.DeletePriceAsync(hotelId, ratePlanId, priceId, ct);
        return NoContent();
    }
}
