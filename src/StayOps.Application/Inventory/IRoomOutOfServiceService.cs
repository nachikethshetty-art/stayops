namespace StayOps.Application.Inventory;

public interface IRoomOutOfServiceService
{
    Task<RoomOutOfServicePeriodDto> SetOutOfOrderAsync(Guid hotelId, Guid roomId, SetRoomOutOfOrderRequest request, Guid? userId, CancellationToken ct = default);
    Task<RoomOutOfServicePeriodDto> ReturnToServiceAsync(Guid hotelId, Guid periodId, Guid? userId, CancellationToken ct = default);
    Task<IReadOnlyList<RoomOutOfServicePeriodDto>> GetByHotelAsync(Guid hotelId, bool activeOnly, CancellationToken ct = default);
}
