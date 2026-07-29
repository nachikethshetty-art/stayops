namespace StayOps.Application.Hotels;

public interface IHotelService
{
    /// <summary>SuperAdmin sees every hotel; other roles see only hotels they have UserHotelAccess to.</summary>
    Task<IReadOnlyList<HotelDto>> GetAccessibleHotelsAsync(CancellationToken ct = default);
    Task<HotelDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<HotelDto> CreateAsync(CreateHotelRequest request, CancellationToken ct = default);
    Task<HotelDto> UpdateAsync(Guid id, UpdateHotelRequest request, CancellationToken ct = default);
}
