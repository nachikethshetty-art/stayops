using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Common;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Application.Reservations;

namespace StayOps.Api.Controllers;

[ApiController]
[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager},{Roles.Receptionist}")]
public class ReservationsController(
    IReservationService service, ICancellationService cancellationService, ICheckInCheckOutService stayService, ICurrentUserService currentUser) : ControllerBase
{
    private Guid? CurrentUserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    /// <summary>Reception one-shot booking: creates the hold and immediately confirms it (payment is settled at the desk / billed to folio).</summary>
    [HttpPost("api/v1/hotels/{hotelId:guid}/reservations")]
    public async Task<ActionResult<ReservationDto>> CreateReceptionReservation(Guid hotelId, [FromBody] CreateReceptionReservationRequest request, CancellationToken ct)
    {
        var scoped = request with { HotelId = hotelId };
        return Ok(await service.CreateReceptionReservationAsync(scoped, CurrentUserId, ct));
    }

    /// <summary>Reception two-step alternative: create a hold first (e.g. to show the guest a locked-in price before confirming).</summary>
    [HttpPost("api/v1/hotels/{hotelId:guid}/inventory-holds")]
    public async Task<ActionResult<InventoryHoldDto>> CreateHold(Guid hotelId, [FromBody] CreateHoldRequest request, CancellationToken ct)
    {
        var scoped = request with { HotelId = hotelId, Source = Domain.Enums.BookingSource.Reception };
        return Ok(await service.CreateHoldAsync(scoped, CurrentUserId, ct));
    }

    [HttpPost("api/v1/hotels/{hotelId:guid}/inventory-holds/{holdId:guid}/confirm")]
    public async Task<ActionResult<ReservationDto>> ConfirmHold(Guid hotelId, Guid holdId, [FromBody] ConfirmReservationRequest request, CancellationToken ct)
    {
        var scoped = request with { HoldId = holdId };
        return Ok(await service.ConfirmAsync(scoped, CurrentUserId, ct));
    }

    [HttpGet("api/v1/hotels/{hotelId:guid}/reservations")]
    public async Task<ActionResult<IReadOnlyList<ReservationListItemDto>>> GetByHotel(
        Guid hotelId, [FromQuery] DateOnly? checkInDate, [FromQuery] DateOnly? checkOutDate, CancellationToken ct)
        => Ok(await service.GetByHotelAsync(hotelId, checkInDate, checkOutDate, ct));

    [HttpGet("api/v1/reservations/{id:guid}")]
    public async Task<ActionResult<ReservationDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        if (result is null) return NotFound();

        if (!await currentUser.CanAccessHotelAsync(result.HotelId, ct))
        {
            throw new ForbiddenAccessException($"You do not have access to hotel '{result.HotelId}'.");
        }

        return Ok(result);
    }

    [HttpGet("api/v1/reservations/{id:guid}/night-rates")]
    public async Task<ActionResult<IReadOnlyList<ReservationNightRateDto>>> GetNightRates(Guid id, CancellationToken ct)
    {
        var reservation = await service.GetByIdAsync(id, ct);
        if (reservation is null) return NotFound();

        if (!await currentUser.CanAccessHotelAsync(reservation.HotelId, ct))
        {
            throw new ForbiddenAccessException($"You do not have access to hotel '{reservation.HotelId}'.");
        }

        return Ok(await service.GetNightRatesAsync(id, ct));
    }

    /// <summary>Guest/reception-initiated cancellation. Penalty is computed from the reservation's snapshotted policy and the hotel's local timezone at the moment of the call.</summary>
    [HttpPost("api/v1/reservations/{id:guid}/cancel")]
    public async Task<ActionResult<CancellationDto>> Cancel(Guid id, [FromBody] CancelReservationRequest request, CancellationToken ct)
        => Ok(await cancellationService.CancelAsync(id, request, CurrentUserId, ct));

    /// <summary>Manual no-show marking outside of Night Audit (Night Audit also detects and processes no-shows automatically - see sp_RunNightAudit).</summary>
    [HttpPost("api/v1/reservations/{id:guid}/no-show")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager},{Roles.Receptionist}")]
    public async Task<ActionResult<CancellationDto>> MarkNoShow(Guid id, [FromBody] CancelReservationRequest request, CancellationToken ct)
        => Ok(await cancellationService.MarkNoShowAsync(id, request.Reason, CurrentUserId, ct));

    [HttpGet("api/v1/reservations/{id:guid}/cancellation")]
    public async Task<ActionResult<CancellationDto>> GetCancellation(Guid id, CancellationToken ct)
    {
        var result = await cancellationService.GetByReservationIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Assigns a physical room and opens the folios required for the stay.</summary>
    [HttpPost("api/v1/reservations/{id:guid}/check-in")]
    public async Task<ActionResult<IReadOnlyList<StayFolioSummaryDto>>> CheckIn(Guid id, [FromBody] CheckInRequest request, CancellationToken ct)
        => Ok(await stayService.CheckInAsync(id, request, CurrentUserId, ct));

    /// <summary>Validates the guest folio is settled, closes the stay's folios, and frees/dirties the room.</summary>
    [HttpPost("api/v1/reservations/{id:guid}/check-out")]
    public async Task<ActionResult<IReadOnlyList<StayFolioSummaryDto>>> CheckOut(Guid id, [FromBody] CheckOutRequest request, CancellationToken ct)
        => Ok(await stayService.CheckOutAsync(id, request, CurrentUserId, ct));

    [HttpPost("api/v1/reservations/{id:guid}/move-room")]
    public async Task<IActionResult> MoveRoom(Guid id, [FromBody] MoveRoomRequest request, CancellationToken ct)
    {
        await stayService.MoveRoomAsync(id, request, CurrentUserId, ct);
        return NoContent();
    }
}
