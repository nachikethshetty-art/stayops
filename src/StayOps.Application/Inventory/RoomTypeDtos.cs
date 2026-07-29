namespace StayOps.Application.Inventory;

public record RoomTypeDto(Guid Id, Guid HotelId, string Code, string Name, string Description, int BaseOccupancy, int MaxOccupancy, int MaxChildren, bool IsActive, int RoomCount);

public record CreateRoomTypeRequest(string Code, string Name, string Description, int BaseOccupancy, int MaxOccupancy, int MaxChildren);
public record UpdateRoomTypeRequest(string Name, string Description, int BaseOccupancy, int MaxOccupancy, int MaxChildren, bool IsActive);
