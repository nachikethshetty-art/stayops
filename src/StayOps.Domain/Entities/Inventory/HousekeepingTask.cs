using StayOps.Domain.Common;
using StayOps.Domain.Enums;

namespace StayOps.Domain.Entities.Inventory;

public class HousekeepingTask : BaseEntity
{
    public Guid HotelId { get; set; }
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }

    public HousekeepingTaskType TaskType { get; set; }
    public HousekeepingTaskStatus Status { get; set; } = HousekeepingTaskStatus.Pending;

    public Guid? AssignedToUserId { get; set; }
    public string Notes { get; set; } = string.Empty;

    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
