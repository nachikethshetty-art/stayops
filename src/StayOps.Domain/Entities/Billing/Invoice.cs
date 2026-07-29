using StayOps.Domain.Common;

namespace StayOps.Domain.Entities.Billing;

public class Invoice : BaseEntity
{
    public Guid ReservationId { get; set; }
    public Guid FolioId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }

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

    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
}

public class InvoiceLine : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public Guid? FolioTransactionId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string HsnSac { get; set; } = string.Empty;

    public decimal TaxableValue { get; set; }
    public decimal CgstRate { get; set; }
    public decimal CgstAmount { get; set; }
    public decimal SgstRate { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal IgstRate { get; set; }
    public decimal IgstAmount { get; set; }
    public decimal LineTotal { get; set; }
}
