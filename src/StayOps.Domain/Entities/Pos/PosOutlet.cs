using StayOps.Domain.Common;

namespace StayOps.Domain.Entities.Pos;

/// <summary>A restaurant/bar/spa outlet that can post charges to guest folios. Mock POS adapter - no real hardware integration.</summary>
public class PosOutlet : BaseEntity
{
    public Guid HotelId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>Hashed API key presented by the POS system alongside its JWT, validated against this outlet + hotel.</summary>
    public string ApiKeyHash { get; set; } = string.Empty;

    public decimal DefaultCreditLimit { get; set; } = 50000m;
    public bool IsActive { get; set; } = true;
}
