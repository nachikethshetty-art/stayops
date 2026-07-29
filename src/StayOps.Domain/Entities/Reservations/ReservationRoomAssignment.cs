using StayOps.Domain.Common;

namespace StayOps.Domain.Entities.Reservations;

/// <summary>
/// Physical room assignment, created at check-in (never before). Supports room moves: closing
/// one assignment (setting CheckedOutAtUtc) and opening a new one against a different room.
/// </summary>
public class ReservationRoomAssignment : BaseEntity
{
    public Guid ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    public Guid RoomId { get; set; }

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UnassignedAtUtc { get; set; }
    public string? MoveReason { get; set; }
}
