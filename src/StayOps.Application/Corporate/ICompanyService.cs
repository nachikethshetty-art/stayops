using StayOps.Application.Common.Models;

namespace StayOps.Application.Corporate;

public interface ICompanyService
{
    Task<PagedResult<CompanyDto>> SearchAsync(PagedRequest request, CancellationToken ct = default);
    Task<CompanyDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CompanyDto> CreateAsync(CreateCompanyRequest request, CancellationToken ct = default);
    Task<CompanyDto> UpdateAsync(Guid id, UpdateCompanyRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<CorporateRateContractDto>> GetContractsForHotelAsync(Guid hotelId, CancellationToken ct = default);
    Task<CorporateRateContractDto> CreateContractAsync(Guid hotelId, CreateCorporateRateContractRequest request, CancellationToken ct = default);
    Task<CorporateRateContractDto> UpdateContractAsync(Guid hotelId, Guid contractId, UpdateCorporateRateContractRequest request, CancellationToken ct = default);
}
