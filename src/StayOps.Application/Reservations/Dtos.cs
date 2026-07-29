using StayOps.Domain.Enums;

namespace StayOps.Application.Reservations;

public record AvailabilitySearchRequest(
    Guid HotelId, DateOnly CheckInDate, DateOnly CheckOutDate, int Adults, int Children,
    Guid? RatePlanId, Guid? CompanyId, Guid? TravelAgentId);

public record RoomTypeAvailabilityDto(
    Guid RoomTypeId, string RoomTypeName, int BaseOccupancy, int MaxOccupancy,
    decimal TotalRoomRateExclGst, decimal AverageNightlyRate,
    Guid RatePlanId, string RatePlanName, MealPlanType MealPlan, string RateSource, int AvailableCount);

public record CreateHoldRequest(
    Guid HotelId, Guid RoomTypeId, Guid RatePlanId, DateOnly CheckInDate, DateOnly CheckOutDate,
    int RoomsRequested, int Adults, int Children, BookingSource Source,
    Guid? GuestId, Guid? CompanyId, Guid? TravelAgentId, string IdempotencyKey);

public record InventoryHoldDto(
    Guid HoldId, Guid HotelId, Guid RoomTypeId, Guid RatePlanId, DateOnly CheckInDate, DateOnly CheckOutDate,
    int RoomsRequested, InventoryHoldStatus Status, BookingSource Source, DateTime ExpiresAtUtc,
    Guid? GuestId, Guid? CompanyId, Guid? TravelAgentId, Guid? ReservationId);

public record ConfirmReservationRequest(Guid HoldId, string IdempotencyKey, string? PaymentReference, Guid? GuestId, bool BillRoomChargeToCompany);

public record CreateReceptionReservationRequest(
    Guid HotelId, Guid RoomTypeId, Guid RatePlanId, DateOnly CheckInDate, DateOnly CheckOutDate,
    Guid GuestId, int RoomsRequested, int Adults, int Children, string IdempotencyKey,
    Guid? CompanyId, Guid? TravelAgentId, bool BillRoomChargeToCompany);

public record ReservationDto(
    Guid Id, Guid HotelId, string ReservationNumber, Guid GuestId, Guid? CompanyId, Guid? TravelAgentId,
    Guid RoomTypeId, Guid RatePlanId, DateOnly CheckInDate, DateOnly CheckOutDate, int RoomsBooked,
    int Adults, int Children, ReservationStatus Status, BookingSource Source, Guid? InventoryHoldId,
    string? IdempotencyKey, DateOnly BusinessDateCreated, Guid? CreatedByUserId, bool BillRoomChargeToCompany,
    DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);

public record ReservationNightRateDto(DateOnly StayDate, decimal RoomRate, MealPlanType MealPlan, decimal CgstRate, decimal SgstRate, decimal IgstRate);

public record ReservationListItemDto(
    Guid ReservationId, Guid HotelId, string ReservationNumber, ReservationStatus Status, BookingSource Source,
    DateOnly CheckInDate, DateOnly CheckOutDate, int RoomsBooked, int Adults, int Children,
    Guid GuestId, string GuestName, string GuestPhone, string GuestEmail,
    Guid RoomTypeId, string RoomTypeName, Guid RatePlanId, string RatePlanName,
    Guid? CompanyId, string? CompanyName, DateTime CreatedAtUtc);
