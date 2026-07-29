using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Common;
using StayOps.Application.Common.Models;
using StayOps.Application.Corporate;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/v1/companies")]
[Authorize]
public class CompaniesController(ICompanyService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<CompanyDto>>> Search([FromQuery] PagedRequest request, CancellationToken ct)
        => Ok(await service.SearchAsync(request, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CompanyDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager},{Roles.FinanceUser}")]
    public async Task<ActionResult<CompanyDto>> Create([FromBody] CreateCompanyRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager},{Roles.FinanceUser}")]
    public async Task<ActionResult<CompanyDto>> Update(Guid id, [FromBody] UpdateCompanyRequest request, CancellationToken ct)
        => Ok(await service.UpdateAsync(id, request, ct));
}

[ApiController]
[Route("api/v1/hotels/{hotelId:guid}/corporate-contracts")]
[Authorize]
public class CorporateContractsController(ICompanyService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CorporateRateContractDto>>> GetAll(Guid hotelId, CancellationToken ct)
        => Ok(await service.GetContractsForHotelAsync(hotelId, ct));

    [HttpPost]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager},{Roles.FinanceUser}")]
    public async Task<ActionResult<CorporateRateContractDto>> Create(Guid hotelId, [FromBody] CreateCorporateRateContractRequest request, CancellationToken ct)
        => Ok(await service.CreateContractAsync(hotelId, request, ct));

    [HttpPut("{contractId:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager},{Roles.FinanceUser}")]
    public async Task<ActionResult<CorporateRateContractDto>> Update(Guid hotelId, Guid contractId, [FromBody] UpdateCorporateRateContractRequest request, CancellationToken ct)
        => Ok(await service.UpdateContractAsync(hotelId, contractId, request, ct));
}
