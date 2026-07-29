using Microsoft.EntityFrameworkCore;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Application.Common.Models;
using StayOps.Domain.Entities.Corporate;

namespace StayOps.Application.Corporate;

public class CompanyService(IApplicationDbContext db) : ICompanyService
{
    public async Task<PagedResult<CompanyDto>> SearchAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = db.Companies.AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(c => c.Name.Contains(term) || c.Gstin.Contains(term));
        }

        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(c => c.Name)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(ToDtoExpression).ToListAsync(ct);

        return new PagedResult<CompanyDto> { Items = items, Page = request.Page, PageSize = request.PageSize, TotalCount = total };
    }

    public async Task<CompanyDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Companies.Where(c => c.Id == id).Select(ToDtoExpression).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Company), id);
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyRequest request, CancellationToken ct = default)
    {
        var company = new Company
        {
            Name = request.Name,
            Gstin = request.Gstin,
            StateCode = request.StateCode,
            BillingAddress = request.BillingAddress,
            CreditLimit = request.CreditLimit
        };
        db.Companies.Add(company);
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(company.Id, ct);
    }

    public async Task<CompanyDto> UpdateAsync(Guid id, UpdateCompanyRequest request, CancellationToken ct = default)
    {
        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException(nameof(Company), id);

        company.Name = request.Name;
        company.Gstin = request.Gstin;
        company.StateCode = request.StateCode;
        company.BillingAddress = request.BillingAddress;
        company.CreditLimit = request.CreditLimit;
        company.IsActive = request.IsActive;
        company.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<IReadOnlyList<CorporateRateContractDto>> GetContractsForHotelAsync(Guid hotelId, CancellationToken ct = default)
    {
        return await db.CorporateRateContracts
            .Where(c => c.HotelId == hotelId)
            .Select(ToContractDtoExpression)
            .ToListAsync(ct);
    }

    public async Task<CorporateRateContractDto> CreateContractAsync(Guid hotelId, CreateCorporateRateContractRequest request, CancellationToken ct = default)
    {
        var companyExists = await db.Companies.AnyAsync(c => c.Id == request.CompanyId, ct);
        if (!companyExists) throw new NotFoundException(nameof(Company), request.CompanyId);

        var ratePlanValid = await db.RatePlans.AnyAsync(rp => rp.Id == request.RatePlanId && rp.HotelId == hotelId, ct);
        if (!ratePlanValid) throw new NotFoundException("RatePlan", request.RatePlanId);

        var contract = new CorporateRateContract
        {
            CompanyId = request.CompanyId,
            HotelId = hotelId,
            RatePlanId = request.RatePlanId,
            ContractStart = request.ContractStart,
            ContractEnd = request.ContractEnd,
            DiscountPercent = request.DiscountPercent,
            BillToCompanyByDefault = request.BillToCompanyByDefault
        };
        db.CorporateRateContracts.Add(contract);
        await db.SaveChangesAsync(ct);

        return await db.CorporateRateContracts.Where(c => c.Id == contract.Id).Select(ToContractDtoExpression).FirstAsync(ct);
    }

    public async Task<CorporateRateContractDto> UpdateContractAsync(Guid hotelId, Guid contractId, UpdateCorporateRateContractRequest request, CancellationToken ct = default)
    {
        var contract = await db.CorporateRateContracts.FirstOrDefaultAsync(c => c.Id == contractId && c.HotelId == hotelId, ct)
            ?? throw new NotFoundException(nameof(CorporateRateContract), contractId);

        contract.ContractStart = request.ContractStart;
        contract.ContractEnd = request.ContractEnd;
        contract.DiscountPercent = request.DiscountPercent;
        contract.BillToCompanyByDefault = request.BillToCompanyByDefault;
        contract.IsActive = request.IsActive;
        contract.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return await db.CorporateRateContracts.Where(c => c.Id == contractId).Select(ToContractDtoExpression).FirstAsync(ct);
    }

    private static readonly System.Linq.Expressions.Expression<Func<Company, CompanyDto>> ToDtoExpression = c => new CompanyDto(
        c.Id, c.Name, c.Gstin, c.StateCode, c.BillingAddress, c.CreditLimit, c.IsActive);

    private static readonly System.Linq.Expressions.Expression<Func<CorporateRateContract, CorporateRateContractDto>> ToContractDtoExpression = c => new CorporateRateContractDto(
        c.Id, c.CompanyId, c.Company!.Name, c.HotelId, c.RatePlanId, c.RatePlan!.Name,
        c.ContractStart, c.ContractEnd, c.DiscountPercent, c.BillToCompanyByDefault, c.IsActive);
}
