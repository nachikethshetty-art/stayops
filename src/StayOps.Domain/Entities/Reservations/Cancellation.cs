using StayOps.Domain.Common;
using StayOps.Domain.Enums;

namespace StayOps.Domain.Entities.Reservations;

public class Cancellation : BaseEntity
{
    public Guid ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    public CancellationTriggerType TriggerType { get; set; }
    public DateTime CancelledAtUtc { get; set; } = DateTime.UtcNow;
    public DateOnly HotelBusinessDateAtCancellation { get; set; }
    public Guid? CancelledByUserId { get; set; }

    public Guid AppliedPolicyRuleId { get; set; }
    public int HoursBeforeCheckIn { get; set; }

    public decimal StayGrossAmount { get; set; }
    public decimal PenaltyAmount { get; set; }
    public decimal PenaltyGstAmount { get; set; }
    public decimal RefundDueAmount { get; set; }

    public string Reason { get; set; } = string.Empty;

    public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}
