using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Common;
using StayOps.Application.Inventory;
using StayOps.Domain.Enums;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/v1/hotels/{hotelId:guid}/rooms")]
[Authorize]
public class RoomsController(IRoomService service, IRoomOutOfServiceService oosService) : ControllerBase
{
    private Guid? CurrentUserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoomDto>>> GetAll(Guid hotelId, [FromQuery] RoomStatus? status, [FromQuery] Guid? roomTypeId, CancellationToken ct)
        => Ok(await service.GetByHotelAsync(hotelId, status, roomTypeId, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoomDto>> GetById(Guid hotelId, Guid id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(hotelId, id, ct));

    [HttpPost]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager}")]
    public async Task<ActionResult<RoomDto>> Create(Guid hotelId, [FromBody] CreateRoomRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(hotelId, request, ct);
        return CreatedAtAction(nameof(GetById), new { hotelId, id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager}")]
    public async Task<ActionResult<RoomDto>> Update(Guid hotelId, Guid id, [FromBody] UpdateRoomRequest request, CancellationToken ct)
        => Ok(await service.UpdateAsync(hotelId, id, request, ct));

    /// <summary>Manual day-to-day status changes (Available/Reserved/Occupied/Dirty). OOO/OOS go through the dedicated approval workflow.</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager},{Roles.Receptionist},{Roles.Housekeeper}")]
    public async Task<ActionResult<RoomDto>> ChangeStatus(Guid hotelId, Guid id, [FromBody] ChangeRoomStatusRequest request, CancellationToken ct)
        => Ok(await service.ChangeStatusAsync(hotelId, id, request, ct));

    [HttpGet("out-of-service")]
    public async Task<ActionResult<IReadOnlyList<RoomOutOfServicePeriodDto>>> GetOutOfServicePeriods(Guid hotelId, [FromQuery] bool activeOnly, CancellationToken ct)
        => Ok(await oosService.GetByHotelAsync(hotelId, activeOnly, ct));

    [HttpPost("{id:guid}/out-of-service")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager}")]
    public async Task<ActionResult<RoomOutOfServicePeriodDto>> SetOutOfOrder(Guid hotelId, Guid id, [FromBody] SetRoomOutOfOrderRequest request, CancellationToken ct)
        => Ok(await oosService.SetOutOfOrderAsync(hotelId, id, request, CurrentUserId, ct));

    [HttpPost("out-of-service/{periodId:guid}/return-to-service")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager}")]
    public async Task<ActionResult<RoomOutOfServicePeriodDto>> ReturnToService(Guid hotelId, Guid periodId, CancellationToken ct)
        => Ok(await oosService.ReturnToServiceAsync(hotelId, periodId, CurrentUserId, ct));
}
