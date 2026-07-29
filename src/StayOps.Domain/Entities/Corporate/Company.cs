using StayOps.Domain.Common;

namespace StayOps.Domain.Entities.Corporate;

/// <summary>A corporate account that can be billed directly (Direct Bill folio) and/or hold negotiated rate contracts.</summary>
public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Gstin { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;

    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CorporateRateContract> Contracts { get; set; } = new List<CorporateRateContract>();
}
