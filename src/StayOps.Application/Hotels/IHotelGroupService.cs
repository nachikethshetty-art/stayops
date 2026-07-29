namespace StayOps.Application.Hotels;

public interface IHotelGroupService
{
    Task<IReadOnlyList<HotelGroupDto>> GetAllAsync(CancellationToken ct = default);
    Task<HotelGroupDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<HotelGroupDto> CreateAsync(CreateHotelGroupRequest request, CancellationToken ct = default);
    Task<HotelGroupDto> UpdateAsync(Guid id, UpdateHotelGroupRequest request, CancellationToken ct = default);
}
