using StayOps.Domain.Common;
using StayOps.Domain.Enums;

namespace StayOps.Domain.Entities.Billing;

/// <summary>
/// A payment can exist before any folio does (e.g. the online-booking gateway payment collected
/// at reservation-confirmation time, well before check-in creates a folio) - hence ReservationId
/// is the required anchor and FolioId is nullable, populated once the payment is reconciled onto
/// a folio (typically at check-in).
/// </summary>
public class Payment : BaseEntity
{
    public Guid ReservationId { get; set; }
    public Guid? FolioId { get; set; }

    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public string? GatewayReference { get; set; }

    /// <summary>Idempotency key for online-payment confirmation webhooks - a retried webhook returns the prior result.</summary>
    public string? IdempotencyKey { get; set; }

    public Guid? RecordedByUserId { get; set; }
    public Guid? FolioTransactionId { get; set; }
}
