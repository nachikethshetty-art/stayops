using StayOps.Domain.Common;
using StayOps.Domain.Entities.Rates;

namespace StayOps.Domain.Entities.Corporate;

/// <summary>
/// A negotiated rate contract between a Company and a hotel. When eligible (active, within date range,
/// matches hotel/room type), this takes top priority in rate selection over agent/public rates.
/// </summary>
public class CorporateRateContract : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public Guid HotelId { get; set; }
    public Guid RatePlanId { get; set; }
    public RatePlan? RatePlan { get; set; }

    public DateOnly ContractStart { get; set; }
    public DateOnly ContractEnd { get; set; }

    /// <summary>Optional flat discount applied on top of the linked rate plan's matrix price.</summary>
    public decimal? DiscountPercent { get; set; }

    public bool IsActive { get; set; } = true;
    public bool BillToCompanyByDefault { get; set; } = true;
}
