namespace StayOps.Infrastructure.Reservations;

/*
    Dapper's fast-path constructor materialization for immutable records requires an exact type
    match per column (no implicit DateTime -> DateOnly conversion). These mutable row classes take
    the raw ADO.NET types (DateTime for SQL `date` columns) and are mapped to the Application
    layer's DateOnly-based DTOs by hand in the service methods below.
*/

public class InventoryHoldRow
{
    public Guid HoldId { get; set; }
    public Guid HotelId { get; set; }
    public Guid RoomTypeId { get; set; }
    public Guid RatePlanId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int RoomsRequested { get; set; }
    public int Status { get; set; }
    public int Source { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public Guid? GuestId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? TravelAgentId { get; set; }
    public Guid? ReservationId { get; set; }
}

public class ReservationRow
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public string ReservationNumber { get; set; } = string.Empty;
    public Guid GuestId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? TravelAgentId { get; set; }
    public Guid RoomTypeId { get; set; }
    public Guid RatePlanId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int RoomsBooked { get; set; }
    public int Adults { get; set; }
    public int Children { get; set; }
    public int Status { get; set; }
    public int Source { get; set; }
    public Guid? InventoryHoldId { get; set; }
    public string? IdempotencyKey { get; set; }
    public DateTime BusinessDateCreated { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public bool BillRoomChargeToCompany { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public class ReservationNightRateRow
{
    public DateTime StayDate { get; set; }
    public decimal RoomRate { get; set; }
    public int MealPlan { get; set; }
    public decimal CgstRate { get; set; }
    public decimal SgstRate { get; set; }
    public decimal IgstRate { get; set; }
}

public class ReservationListItemRow
{
    public Guid ReservationId { get; set; }
    public Guid HotelId { get; set; }
    public string ReservationNumber { get; set; } = string.Empty;
    public int Status { get; set; }
    public int Source { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int RoomsBooked { get; set; }
    public int Adults { get; set; }
    public int Children { get; set; }
    public Guid GuestId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public Guid RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public Guid RatePlanId { get; set; }
    public string RatePlanName { get; set; } = string.Empty;
    public Guid? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
