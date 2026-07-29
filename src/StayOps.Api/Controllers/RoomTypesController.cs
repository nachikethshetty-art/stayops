using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Common;
using StayOps.Application.Inventory;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/v1/hotels/{hotelId:guid}/room-types")]
[Authorize]
public class RoomTypesController(IRoomTypeService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoomTypeDto>>> GetAll(Guid hotelId, CancellationToken ct)
        => Ok(await service.GetByHotelAsync(hotelId, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoomTypeDto>> GetById(Guid hotelId, Guid id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(hotelId, id, ct));

    [HttpPost]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager}")]
    public async Task<ActionResult<RoomTypeDto>> Create(Guid hotelId, [FromBody] CreateRoomTypeRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(hotelId, request, ct);
        return CreatedAtAction(nameof(GetById), new { hotelId, id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager}")]
    public async Task<ActionResult<RoomTypeDto>> Update(Guid hotelId, Guid id, [FromBody] UpdateRoomTypeRequest request, CancellationToken ct)
        => Ok(await service.UpdateAsync(hotelId, id, request, ct));
}
