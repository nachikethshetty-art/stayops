using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Common;
using StayOps.Application.Hotels;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/v1/hotel-groups")]
[Authorize(Roles = Roles.SuperAdmin)]
public class HotelGroupsController(IHotelGroupService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HotelGroupDto>>> GetAll(CancellationToken ct)
        => Ok(await service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HotelGroupDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<HotelGroupDto>> Create([FromBody] CreateHotelGroupRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<HotelGroupDto>> Update(Guid id, [FromBody] UpdateHotelGroupRequest request, CancellationToken ct)
        => Ok(await service.UpdateAsync(id, request, ct));
}
