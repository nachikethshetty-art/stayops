namespace StayOps.Application.Inventory;

public interface IRoomTypeService
{
    Task<IReadOnlyList<RoomTypeDto>> GetByHotelAsync(Guid hotelId, CancellationToken ct = default);
    Task<RoomTypeDto> GetByIdAsync(Guid hotelId, Guid id, CancellationToken ct = default);
    Task<RoomTypeDto> CreateAsync(Guid hotelId, CreateRoomTypeRequest request, CancellationToken ct = default);
    Task<RoomTypeDto> UpdateAsync(Guid hotelId, Guid id, UpdateRoomTypeRequest request, CancellationToken ct = default);
}
