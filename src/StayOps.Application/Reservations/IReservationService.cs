namespace StayOps.Application.Reservations;

public interface IReservationService
{
    Task<InventoryHoldDto> CreateHoldAsync(CreateHoldRequest request, Guid? createdByUserId, CancellationToken ct = default);
    Task<ReservationDto> ConfirmAsync(ConfirmReservationRequest request, Guid? createdByUserId, CancellationToken ct = default);
    Task<ReservationDto> CreateReceptionReservationAsync(CreateReceptionReservationRequest request, Guid? createdByUserId, CancellationToken ct = default);
    Task<ReservationDto?> GetByIdAsync(Guid reservationId, CancellationToken ct = default);
    Task<IReadOnlyList<ReservationListItemDto>> GetByHotelAsync(Guid hotelId, DateOnly? checkInDate, DateOnly? checkOutDate, CancellationToken ct = default);
    Task<IReadOnlyList<ReservationNightRateDto>> GetNightRatesAsync(Guid reservationId, CancellationToken ct = default);
}
