using StayOps.Domain.Common;
using StayOps.Domain.Enums;

namespace StayOps.Domain.Entities.Billing;

/// <summary>
/// One stay can have up to three folios open simultaneously: Guest (incidentals, default),
/// Company (room rent routed here when a corporate contract says BillToCompanyByDefault),
/// and DirectBill (ad-hoc direct-bill arrangement independent of a contract).
/// </summary>
public class Folio : BaseEntity
{
    public Guid ReservationId { get; set; }
    public FolioType Type { get; set; }
    public Guid? OwnerCompanyId { get; set; }

    public FolioStatus Status { get; set; } = FolioStatus.Open;
    public decimal Balance { get; set; }

    public DateTime OpenedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAtUtc { get; set; }

    public ICollection<FolioTransaction> Transactions { get; set; } = new List<FolioTransaction>();
}
