using StayOps.Domain.Common;
using StayOps.Domain.Enums;

namespace StayOps.Domain.Entities.Billing;

/// <summary>
/// Immutable ledger line. Corrections are made by posting a Reversal transaction that references
/// the original (ReversalOfTransactionId) - existing rows are never edited or deleted.
/// UniquePostingKey enforces "post at most once" for automated postings (Night Audit room charge
/// per stay/date, POS charge per outlet+reference) via a unique index in SQL.
/// </summary>
public class FolioTransaction : BaseEntity
{
    public Guid FolioId { get; set; }
    public Folio? Folio { get; set; }

    public FolioTransactionType Type { get; set; }
    public string Description { get; set; } = string.Empty;

    /// <summary>Taxable value before GST for charge lines; gross amount for payments.</summary>
    public decimal Amount { get; set; }
    public decimal GstAmount { get; set; }

    /// <summary>Amount + GstAmount for charges; -(Amount) for payments/refunds (credits are negative).</summary>
    public decimal TotalAmount { get; set; }

    public Guid? ReversalOfTransactionId { get; set; }

    public DateOnly BusinessDate { get; set; }
    public Guid? PostedByUserId { get; set; }
    public string? SourceReference { get; set; }

    /// <summary>Nullable, unique when present - see FolioTransactionConfiguration for the filtered unique index.</summary>
    public string? UniquePostingKey { get; set; }

    public ICollection<FolioTaxLine> TaxLines { get; set; } = new List<FolioTaxLine>();
}
