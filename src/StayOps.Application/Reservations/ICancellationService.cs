namespace StayOps.Application.Reservations;

public interface ICancellationService
{
    Task<CancellationDto> CancelAsync(Guid reservationId, CancelReservationRequest request, Guid? cancelledByUserId, CancellationToken ct = default);
    Task<CancellationDto> MarkNoShowAsync(Guid reservationId, string? reason, Guid? triggeredByUserId, CancellationToken ct = default);
    Task<CancellationDto?> GetByReservationIdAsync(Guid reservationId, CancellationToken ct = default);
}
