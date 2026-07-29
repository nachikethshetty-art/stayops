using StayOps.Domain.Common;
using StayOps.Domain.Enums;

namespace StayOps.Domain.Entities.Reservations;

public class Reservation : BaseEntity
{
    public Guid HotelId { get; set; }
    public string ReservationNumber { get; set; } = string.Empty;

    public Guid GuestId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? TravelAgentId { get; set; }

    public Guid RoomTypeId { get; set; }
    public Guid RatePlanId { get; set; }

    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public int RoomsBooked { get; set; } = 1;
    public int Adults { get; set; } = 1;
    public int Children { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.PendingPayment;
    public BookingSource Source { get; set; }

    public Guid? InventoryHoldId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;

    public DateOnly BusinessDateCreated { get; set; }
    public Guid? CreatedByUserId { get; set; }

    public bool BillRoomChargeToCompany { get; set; }

    public ICollection<ReservationRoomAssignment> RoomAssignments { get; set; } = new List<ReservationRoomAssignment>();
    public ICollection<ReservationNightRate> NightRates { get; set; } = new List<ReservationNightRate>();
    public ReservationPolicySnapshot? PolicySnapshot { get; set; }
}
