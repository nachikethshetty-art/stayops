using StayOps.Domain.Common;
using StayOps.Domain.Enums;

namespace StayOps.Domain.Entities.Billing;

/// <summary>
/// Configurable, effective-dated GST rule. Indian hotel-room GST is tariff-slab based (rate depends
/// on the per-night room rate), so a rule optionally scopes to [MinAmount, MaxAmount). CGST+SGST
/// apply when hotel state == billed party state; IGST applies otherwise. Never hard-code rates in C#.
/// </summary>
public class GstRule : BaseEntity
{
    public Guid? HotelId { get; set; }

    public GstChargeCategory ChargeCategory { get; set; }
    public string HsnSac { get; set; } = string.Empty;

    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }

    public decimal CgstRate { get; set; }
    public decimal SgstRate { get; set; }
    public decimal IgstRate { get; set; }

    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }

    public bool IsActive { get; set; } = true;
}
