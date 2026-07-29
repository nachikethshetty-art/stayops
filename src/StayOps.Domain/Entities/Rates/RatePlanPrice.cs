using StayOps.Domain.Common;

namespace StayOps.Domain.Entities.Rates;

/// <summary>
/// Date-effective rate matrix row: one price for a given room type + occupancy + weekday,
/// valid for [EffectiveFrom, EffectiveTo]. DayOfWeek is null to mean "applies every day of week"
/// unless a more specific weekday row also matches, in which case the most specific row wins
/// (see RateResolutionService for the exact precedence rule).
/// </summary>
public class RatePlanPrice : BaseEntity
{
    public Guid RatePlanId { get; set; }
    public RatePlan? RatePlan { get; set; }

    public Guid RoomTypeId { get; set; }

    public int Occupancy { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }

    public DateOnly EffectiveFrom { get; set; }
    public DateOnly EffectiveTo { get; set; }

    public decimal Rate { get; set; }
}
