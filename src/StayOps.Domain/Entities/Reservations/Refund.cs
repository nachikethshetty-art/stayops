using StayOps.Domain.Common;
using StayOps.Domain.Enums;

namespace StayOps.Domain.Entities.Reservations;

/// <summary>
/// A refund is always a separate, additive financial entry - it never overwrites or edits the
/// original payment record. Lifecycle: RefundRequested -> Approved -> SentToGateway -> Succeeded/Failed.
/// The gateway call is a mock adapter (see StayOps.Application/Payments) with simulated async status.
/// </summary>
public class Refund : BaseEntity
{
    public Guid CancellationId { get; set; }
    public Cancellation? Cancellation { get; set; }

    public Guid ReservationId { get; set; }
    public Guid? OriginalPaymentId { get; set; }

    public decimal Amount { get; set; }
    public RefundStatus Status { get; set; } = RefundStatus.RefundRequested;

    public string? GatewayReference { get; set; }
    public string? FailureReason { get; set; }

    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAtUtc { get; set; }
    public DateTime? SentToGatewayAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public Guid? ApprovedByUserId { get; set; }
}
