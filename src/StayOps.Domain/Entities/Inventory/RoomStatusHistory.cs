using StayOps.Domain.Common;
using StayOps.Domain.Enums;

namespace StayOps.Domain.Entities.Inventory;

public class RoomStatusHistory : BaseEntity
{
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }

    public RoomStatus FromStatus { get; set; }
    public RoomStatus ToStatus { get; set; }
    public string? Reason { get; set; }
    public Guid? ChangedByUserId { get; set; }
    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
}
