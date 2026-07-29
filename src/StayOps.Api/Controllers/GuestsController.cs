using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Common.Models;
using StayOps.Application.Guests;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/v1/guests")]
[Authorize]
public class GuestsController(IGuestService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<GuestDto>>> Search([FromQuery] PagedRequest request, CancellationToken ct)
        => Ok(await service.SearchAsync(request, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GuestDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<GuestDto>> Create([FromBody] CreateGuestRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GuestDto>> Update(Guid id, [FromBody] UpdateGuestRequest request, CancellationToken ct)
        => Ok(await service.UpdateAsync(id, request, ct));
}
