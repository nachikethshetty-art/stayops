using StayOps.Domain.Common;
using StayOps.Domain.Entities.Organization;
using StayOps.Domain.Enums;

namespace StayOps.Domain.Entities.Inventory;

public class Room : BaseEntity
{
    public Guid HotelId { get; set; }
    public Hotel? Hotel { get; set; }

    public Guid RoomTypeId { get; set; }
    public RoomType? RoomType { get; set; }

    public string RoomNumber { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;

    public RoomStatus Status { get; set; } = RoomStatus.Available;

    /// <summary>Active rooms participate in inventory/occupancy math; inactive rooms are permanently retired (e.g. demolished).</summary>
    public bool IsActive { get; set; } = true;

    public ICollection<RoomStatusHistory> StatusHistory { get; set; } = new List<RoomStatusHistory>();
}
