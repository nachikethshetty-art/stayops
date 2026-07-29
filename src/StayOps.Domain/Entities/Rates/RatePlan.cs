using StayOps.Domain.Common;
using StayOps.Domain.Entities.CancellationPolicies;
using StayOps.Domain.Enums;

namespace StayOps.Domain.Entities.Rates;

public class RatePlan : BaseEntity
{
    public Guid HotelId { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public MealPlanType MealPlan { get; set; }
    public RatePlanScope Scope { get; set; }

    public Guid? CancellationPolicyId { get; set; }
    public CancellationPolicy? CancellationPolicy { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<RatePlanPrice> Prices { get; set; } = new List<RatePlanPrice>();
}
