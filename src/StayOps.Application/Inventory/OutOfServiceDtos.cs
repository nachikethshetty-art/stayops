using StayOps.Domain.Enums;

namespace StayOps.Application.Inventory;

public record SetRoomOutOfOrderRequest(RoomOutOfServiceType Type, DateOnly StartDate, DateOnly EndDate, string Reason);

public record RoomOutOfServicePeriodDto(
    Guid Id, Guid RoomId, string RoomNumber, RoomOutOfServiceType Type, DateOnly StartDate, DateOnly EndDate,
    string Reason, OutOfServiceStatus Status, DateTime? ApprovedAtUtc, DateTime? ReturnedToServiceAtUtc);
