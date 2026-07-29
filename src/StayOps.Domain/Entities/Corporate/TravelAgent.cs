using StayOps.Domain.Common;
using StayOps.Domain.Entities.Rates;

namespace StayOps.Domain.Entities.Corporate;

/// <summary>Travel-agent entity and contract model. No live OTA integration in this release - manual/offline contracts only.</summary>
public class TravelAgent : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Gstin { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public decimal CommissionPercent { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<AgentRateContract> Contracts { get; set; } = new List<AgentRateContract>();
}

public class AgentRateContract : BaseEntity
{
    public Guid TravelAgentId { get; set; }
    public TravelAgent? TravelAgent { get; set; }

    public Guid HotelId { get; set; }
    public Guid RatePlanId { get; set; }
    public RatePlan? RatePlan { get; set; }

    public DateOnly ContractStart { get; set; }
    public DateOnly ContractEnd { get; set; }
    public decimal? DiscountPercent { get; set; }
    public bool IsActive { get; set; } = true;
}
