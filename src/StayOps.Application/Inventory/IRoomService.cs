using StayOps.Domain.Enums;

namespace StayOps.Application.Inventory;

public interface IRoomService
{
    Task<IReadOnlyList<RoomDto>> GetByHotelAsync(Guid hotelId, RoomStatus? status, Guid? roomTypeId, CancellationToken ct = default);
    Task<RoomDto> GetByIdAsync(Guid hotelId, Guid id, CancellationToken ct = default);
    Task<RoomDto> CreateAsync(Guid hotelId, CreateRoomRequest request, CancellationToken ct = default);
    Task<RoomDto> UpdateAsync(Guid hotelId, Guid id, UpdateRoomRequest request, CancellationToken ct = default);
    Task<RoomDto> ChangeStatusAsync(Guid hotelId, Guid id, ChangeRoomStatusRequest request, CancellationToken ct = default);
}
