using Microsoft.EntityFrameworkCore;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Domain.Entities.Organization;

namespace StayOps.Application.Hotels;

public class HotelGroupService(IApplicationDbContext db) : IHotelGroupService
{
    public async Task<IReadOnlyList<HotelGroupDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.HotelGroups
            .Select(g => new HotelGroupDto(g.Id, g.Name, g.IsActive, g.Hotels.Count))
            .ToListAsync(ct);
    }

    public async Task<HotelGroupDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.HotelGroups
            .Where(g => g.Id == id)
            .Select(g => new HotelGroupDto(g.Id, g.Name, g.IsActive, g.Hotels.Count))
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(HotelGroup), id);
    }

    public async Task<HotelGroupDto> CreateAsync(CreateHotelGroupRequest request, CancellationToken ct = default)
    {
        var group = new HotelGroup { Name = request.Name };
        db.HotelGroups.Add(group);
        await db.SaveChangesAsync(ct);
        return new HotelGroupDto(group.Id, group.Name, group.IsActive, 0);
    }

    public async Task<HotelGroupDto> UpdateAsync(Guid id, UpdateHotelGroupRequest request, CancellationToken ct = default)
    {
        var group = await db.HotelGroups.FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new NotFoundException(nameof(HotelGroup), id);

        group.Name = request.Name;
        group.IsActive = request.IsActive;
        group.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }
}
