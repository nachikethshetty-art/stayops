using StayOps.Domain.Common;

namespace StayOps.Domain.Entities.Billing;

/// <summary>Audit record of a charge transfer between folios (e.g. guest folio -> company folio for room rent).</summary>
public class FolioTransfer : BaseEntity
{
    public Guid FromFolioId { get; set; }
    public Guid ToFolioId { get; set; }

    public Guid SourceReversalTransactionId { get; set; }
    public Guid DestinationTransactionId { get; set; }

    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;

    public Guid TransferredByUserId { get; set; }
    public DateTime TransferredAtUtc { get; set; } = DateTime.UtcNow;
}
