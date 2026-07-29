using StayOps.Domain.Enums;

namespace StayOps.Application.Housekeeping;

public interface IHousekeepingService
{
    Task<IReadOnlyList<HousekeepingTaskDto>> GetByHotelAsync(Guid hotelId, HousekeepingTaskStatus? status, CancellationToken ct = default);
    Task<HousekeepingTaskDto> CreateAsync(Guid hotelId, CreateHousekeepingTaskRequest request, CancellationToken ct = default);
    Task<HousekeepingTaskDto> UpdateStatusAsync(Guid hotelId, Guid taskId, UpdateHousekeepingTaskStatusRequest request, CancellationToken ct = default);
}
