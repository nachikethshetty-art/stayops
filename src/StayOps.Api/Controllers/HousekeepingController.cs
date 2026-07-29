using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Housekeeping;
using StayOps.Domain.Enums;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/v1/hotels/{hotelId:guid}/housekeeping-tasks")]
[Authorize]
public class HousekeepingController(IHousekeepingService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HousekeepingTaskDto>>> GetAll(Guid hotelId, [FromQuery] HousekeepingTaskStatus? status, CancellationToken ct)
        => Ok(await service.GetByHotelAsync(hotelId, status, ct));

    [HttpPost]
    public async Task<ActionResult<HousekeepingTaskDto>> Create(Guid hotelId, [FromBody] CreateHousekeepingTaskRequest request, CancellationToken ct)
        => Ok(await service.CreateAsync(hotelId, request, ct));

    [HttpPatch("{taskId:guid}/status")]
    public async Task<ActionResult<HousekeepingTaskDto>> UpdateStatus(Guid hotelId, Guid taskId, [FromBody] UpdateHousekeepingTaskStatusRequest request, CancellationToken ct)
        => Ok(await service.UpdateStatusAsync(hotelId, taskId, request, ct));
}
