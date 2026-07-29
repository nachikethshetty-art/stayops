using StayOps.Domain.Common;

namespace StayOps.Domain.Entities.Billing;

/// <summary>GST breakdown for a single folio transaction. Rates are copied from GstRule at posting time - never hard-coded.</summary>
public class FolioTaxLine : BaseEntity
{
    public Guid FolioTransactionId { get; set; }
    public FolioTransaction? FolioTransaction { get; set; }

    public Guid GstRuleId { get; set; }
    public string HsnSac { get; set; } = string.Empty;

    public decimal TaxableValue { get; set; }

    public decimal CgstRate { get; set; }
    public decimal CgstAmount { get; set; }
    public decimal SgstRate { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal IgstRate { get; set; }
    public decimal IgstAmount { get; set; }
}
