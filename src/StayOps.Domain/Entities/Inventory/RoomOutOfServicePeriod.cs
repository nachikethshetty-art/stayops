using StayOps.Domain.Common;
using StayOps.Domain.Enums;

namespace StayOps.Domain.Entities.Inventory;

/// <summary>
/// Tracks OOO (Out Of Order) and OOS (Out Of Service) date ranges for a room.
/// OOO rooms are excluded from sellable inventory AND the occupancy denominator.
/// OOS rooms cannot be booked but remain in the occupancy denominator (documented demo policy).
/// </summary>
public class RoomOutOfServicePeriod : BaseEntity
{
    public Guid RoomId { get; set; }
    public Inventory.Room? Room { get; set; }

    public RoomOutOfServiceType Type { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;

    public OutOfServiceStatus Status { get; set; } = OutOfServiceStatus.PendingApproval;
    public Guid? RequestedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public DateTime? ReturnedToServiceAtUtc { get; set; }
    public Guid? ReturnedByUserId { get; set; }
}
