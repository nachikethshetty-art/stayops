using StayOps.Domain.Enums;

namespace StayOps.Application.Inventory;

public record RoomDto(Guid Id, Guid HotelId, Guid RoomTypeId, string RoomTypeName, string RoomNumber, string Floor, RoomStatus Status, bool IsActive);

public record CreateRoomRequest(Guid RoomTypeId, string RoomNumber, string Floor);
public record UpdateRoomRequest(Guid RoomTypeId, string RoomNumber, string Floor, bool IsActive);
public record ChangeRoomStatusRequest(RoomStatus NewStatus, string? Reason);
