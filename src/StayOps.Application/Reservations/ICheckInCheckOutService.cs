namespace StayOps.Application.Reservations;

public interface ICheckInCheckOutService
{
    Task<IReadOnlyList<StayFolioSummaryDto>> CheckInAsync(Guid reservationId, CheckInRequest request, Guid? userId, CancellationToken ct = default);
    Task<IReadOnlyList<StayFolioSummaryDto>> CheckOutAsync(Guid reservationId, CheckOutRequest request, Guid? userId, CancellationToken ct = default);
    Task MoveRoomAsync(Guid reservationId, MoveRoomRequest request, Guid? userId, CancellationToken ct = default);
}
