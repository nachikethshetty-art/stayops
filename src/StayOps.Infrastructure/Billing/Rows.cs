namespace StayOps.Infrastructure.Billing;

public class FolioTransactionRow
{
    public Guid Id { get; set; }
    public Guid FolioId { get; set; }
    public int Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal GstAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid? ReversalOfTransactionId { get; set; }
    public DateTime BusinessDate { get; set; }
    public Guid? PostedByUserId { get; set; }
    public string? SourceReference { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class InvoiceRow
{
    public Guid Id { get; set; }
    public Guid ReservationId { get; set; }
    public Guid FolioId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string SupplierGstin { get; set; } = string.Empty;
    public string SupplierStateCode { get; set; } = string.Empty;
    public string? BilledPartyName { get; set; }
    public string? BilledPartyGstin { get; set; }
    public string BilledPartyStateCode { get; set; } = string.Empty;
    public string PlaceOfSupplyStateCode { get; set; } = string.Empty;
    public bool IsInterState { get; set; }
    public decimal TotalTaxableValue { get; set; }
    public decimal TotalCgst { get; set; }
    public decimal TotalSgst { get; set; }
    public decimal TotalIgst { get; set; }
    public decimal TotalAmount { get; set; }
}
