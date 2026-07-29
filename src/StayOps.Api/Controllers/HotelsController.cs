using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Common;
using StayOps.Application.Hotels;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/v1/hotels")]
[Authorize]
public class HotelsController(IHotelService service) : ControllerBase
{
    /// <summary>SuperAdmin gets every hotel; other roles get only the hotels they have access to.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HotelDto>>> GetAccessible(CancellationToken ct)
        => Ok(await service.GetAccessibleHotelsAsync(ct));

    [HttpGet("{hotelId:guid}")]
    public async Task<ActionResult<HotelDto>> GetById(Guid hotelId, CancellationToken ct)
        => Ok(await service.GetByIdAsync(hotelId, ct));

    [HttpPost]
    [Authorize(Roles = Roles.SuperAdmin)]
    public async Task<ActionResult<HotelDto>> Create([FromBody] CreateHotelRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { hotelId = result.Id }, result);
    }

    [HttpPut("{hotelId:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager}")]
    public async Task<ActionResult<HotelDto>> Update(Guid hotelId, [FromBody] UpdateHotelRequest request, CancellationToken ct)
        => Ok(await service.UpdateAsync(hotelId, request, ct));
}
