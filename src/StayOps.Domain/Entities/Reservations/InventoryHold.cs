using StayOps.Domain.Common;
using StayOps.Domain.Enums;

namespace StayOps.Domain.Entities.Reservations;

/// <summary>
/// A short-lived reservation of room-type inventory (not a physical room) created by
/// sp_CreateInventoryHold. Online bookings and reception bookings both go through this same
/// hold -> confirm workflow so there is exactly one inventory counting mechanism.
/// </summary>
public class InventoryHold : BaseEntity
{
    public Guid HotelId { get; set; }
    public Guid RoomTypeId { get; set; }
    public Guid RatePlanId { get; set; }

    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public int RoomsRequested { get; set; } = 1;
    public int Adults { get; set; } = 1;
    public int Children { get; set; }

    public InventoryHoldStatus Status { get; set; } = InventoryHoldStatus.Active;
    public BookingSource Source { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>Client-supplied idempotency key so retried "create hold" calls don't create duplicate holds.</summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    public Guid? GuestId { get; set; }
    public Guid? CreatedByUserId { get; set; }

    /// <summary>Carried through from hold to confirm so rate resolution (corporate/agent/public priority) is identical at both steps.</summary>
    public Guid? CompanyId { get; set; }
    public Guid? TravelAgentId { get; set; }

    public Guid? ReservationId { get; set; }
}
