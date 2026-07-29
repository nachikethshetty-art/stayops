using StayOps.Domain.Enums;

namespace StayOps.Application.Reservations;

public record StayFolioSummaryDto(Guid FolioId, FolioType FolioType, FolioStatus FolioStatus, decimal Balance);

public record CheckInRequest(Guid RoomId);
public record CheckOutRequest(bool ForceCheckout);
public record MoveRoomRequest(Guid NewRoomId, string? Reason);
