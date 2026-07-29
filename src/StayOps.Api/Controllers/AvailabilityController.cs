using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Reservations;

namespace StayOps.Api.Controllers;

/// <summary>
/// Availability search shared by the online booking demo screen and the reception workspace -
/// both call this one endpoint, which is backed by the one sp_SearchAvailableRoomTypes procedure.
/// </summary>
[ApiController]
[Route("api/v1/hotels/{hotelId:guid}/availability")]
[AllowAnonymous]
public class AvailabilityController(IAvailabilityService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoomTypeAvailabilityDto>>> Search(
        Guid hotelId, [FromQuery] DateOnly checkInDate, [FromQuery] DateOnly checkOutDate,
        [FromQuery] int adults, [FromQuery] int children, [FromQuery] Guid? ratePlanId,
        [FromQuery] Guid? companyId, [FromQuery] Guid? travelAgentId, CancellationToken ct)
    {
        var request = new AvailabilitySearchRequest(hotelId, checkInDate, checkOutDate, adults, children, ratePlanId, companyId, travelAgentId);
        return Ok(await service.SearchAsync(request, ct));
    }
}
