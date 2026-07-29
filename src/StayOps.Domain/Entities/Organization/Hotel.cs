using StayOps.Domain.Common;
using StayOps.Domain.Entities.Inventory;

namespace StayOps.Domain.Entities.Organization;

/// <summary>
/// A single property. StateCode is the 2-digit GST state code (e.g. "27" = Maharashtra),
/// used to decide CGST+SGST (same state as guest/company) vs IGST (different state) on invoices.
/// BusinessDate is the hotel's current operational date, advanced only by a successful Night Audit run,
/// and is independent of the UTC calendar date used for technical timestamps.
/// </summary>
public class Hotel : BaseEntity
{
    public Guid HotelGroupId { get; set; }
    public HotelGroup? HotelGroup { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;

    /// <summary>2-digit Indian GST state code, e.g. "27".</summary>
    public string StateCode { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;

    /// <summary>15-character GSTIN of the hotel entity.</summary>
    public string Gstin { get; set; } = string.Empty;

    /// <summary>IANA timezone id, e.g. "Asia/Kolkata".</summary>
    public string TimeZoneId { get; set; } = "Asia/Kolkata";

    public DateOnly BusinessDate { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<RoomType> RoomTypes { get; set; } = new List<RoomType>();
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
