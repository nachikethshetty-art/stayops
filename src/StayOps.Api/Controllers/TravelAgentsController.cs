using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Common;
using StayOps.Application.Corporate;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/v1/travel-agents")]
[Authorize]
public class TravelAgentsController(ITravelAgentService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TravelAgentDto>>> GetAll(CancellationToken ct)
        => Ok(await service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TravelAgentDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager},{Roles.FinanceUser}")]
    public async Task<ActionResult<TravelAgentDto>> Create([FromBody] CreateTravelAgentRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager},{Roles.FinanceUser}")]
    public async Task<ActionResult<TravelAgentDto>> Update(Guid id, [FromBody] UpdateTravelAgentRequest request, CancellationToken ct)
        => Ok(await service.UpdateAsync(id, request, ct));
}

[ApiController]
[Route("api/v1/hotels/{hotelId:guid}/agent-contracts")]
[Authorize]
public class AgentContractsController(ITravelAgentService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AgentRateContractDto>>> GetAll(Guid hotelId, CancellationToken ct)
        => Ok(await service.GetContractsForHotelAsync(hotelId, ct));

    [HttpPost]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager},{Roles.FinanceUser}")]
    public async Task<ActionResult<AgentRateContractDto>> Create(Guid hotelId, [FromBody] CreateAgentRateContractRequest request, CancellationToken ct)
        => Ok(await service.CreateContractAsync(hotelId, request, ct));

    [HttpPut("{contractId:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager},{Roles.FinanceUser}")]
    public async Task<ActionResult<AgentRateContractDto>> Update(Guid hotelId, Guid contractId, [FromBody] UpdateAgentRateContractRequest request, CancellationToken ct)
        => Ok(await service.UpdateContractAsync(hotelId, contractId, request, ct));
}
