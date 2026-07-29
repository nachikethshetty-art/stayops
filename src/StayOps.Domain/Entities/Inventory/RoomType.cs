using StayOps.Domain.Common;
using StayOps.Domain.Entities.Organization;

namespace StayOps.Domain.Entities.Inventory;

public class RoomType : BaseEntity
{
    public Guid HotelId { get; set; }
    public Hotel? Hotel { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int BaseOccupancy { get; set; } = 2;
    public int MaxOccupancy { get; set; } = 3;
    public int MaxChildren { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
