using StayOps.Domain.Common;

namespace StayOps.Domain.Entities.Pos;

/// <summary>
/// Records every POST /api/v1/pos/post-charge call. IdempotencyKey = OutletCode + ":" + PosReferenceNumber;
/// a duplicate call with the same key returns the original FolioTransactionId instead of posting again.
/// </summary>
public class PosCharge : BaseEntity
{
    public Guid PosOutletId { get; set; }
    public string PosReferenceNumber { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;

    public Guid RoomId { get; set; }
    public Guid ReservationId { get; set; }
    public Guid FolioId { get; set; }
    public Guid FolioTransactionId { get; set; }

    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}
