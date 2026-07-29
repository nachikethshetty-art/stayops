using StayOps.Application.Common.Models;

namespace StayOps.Application.Guests;

public interface IGuestService
{
    Task<PagedResult<GuestDto>> SearchAsync(PagedRequest request, CancellationToken ct = default);
    Task<GuestDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<GuestDto> CreateAsync(CreateGuestRequest request, CancellationToken ct = default);
    Task<GuestDto> UpdateAsync(Guid id, UpdateGuestRequest request, CancellationToken ct = default);
}
