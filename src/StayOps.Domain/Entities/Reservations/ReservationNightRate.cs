using StayOps.Domain.Common;
using StayOps.Domain.Enums;

namespace StayOps.Domain.Entities.Reservations;

/// <summary>
/// Immutable per-night rate snapshot captured at reservation creation time. A confirmed
/// reservation is NEVER recalculated from later rate-plan or GST-rule changes - Night Audit
/// and invoicing always read from these snapshot rows.
/// </summary>
public class ReservationNightRate : BaseEntity
{
    public Guid ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    public DateOnly StayDate { get; set; }
    public decimal RoomRate { get; set; }
    public MealPlanType MealPlan { get; set; }
    public string InclusionsDescription { get; set; } = string.Empty;

    /// <summary>GST rule id resolved and snapshotted at booking time.</summary>
    public Guid GstRuleId { get; set; }
    public decimal CgstRate { get; set; }
    public decimal SgstRate { get; set; }
    public decimal IgstRate { get; set; }

    public string CurrencyCode { get; set; } = "INR";
}
