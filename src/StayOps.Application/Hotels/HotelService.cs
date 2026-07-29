using Microsoft.EntityFrameworkCore;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Domain.Entities.Organization;

namespace StayOps.Application.Hotels;

public class HotelService(IApplicationDbContext db, ICurrentUserService currentUser) : IHotelService
{
    public async Task<IReadOnlyList<HotelDto>> GetAccessibleHotelsAsync(CancellationToken ct = default)
    {
        var query = db.Hotels.AsQueryable();

        if (!currentUser.IsSuperAdmin)
        {
            var accessibleIds = db.UserHotelAccesses
                .Where(a => a.UserId == currentUser.UserId)
                .Select(a => a.HotelId);
            query = query.Where(h => accessibleIds.Contains(h.Id));
        }

        return await query.Select(ToDtoExpression).ToListAsync(ct);
    }

    public async Task<HotelDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (!await currentUser.CanAccessHotelAsync(id, ct))
        {
            throw new ForbiddenAccessException($"You do not have access to hotel '{id}'.");
        }

        return await db.Hotels.Where(h => h.Id == id).Select(ToDtoExpression).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Hotel), id);
    }

    public async Task<HotelDto> CreateAsync(CreateHotelRequest request, CancellationToken ct = default)
    {
        if (!currentUser.IsSuperAdmin)
        {
            throw new ForbiddenAccessException("Only SuperAdmin can create hotels.");
        }

        var hotel = new Hotel
        {
            HotelGroupId = request.HotelGroupId,
            Code = request.Code,
            Name = request.Name,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            Pincode = request.Pincode,
            StateCode = request.StateCode,
            StateName = request.StateName,
            Gstin = request.Gstin,
            TimeZoneId = request.TimeZoneId,
            BusinessDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        db.Hotels.Add(hotel);
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(hotel.Id, ct);
    }

    public async Task<HotelDto> UpdateAsync(Guid id, UpdateHotelRequest request, CancellationToken ct = default)
    {
        if (!await currentUser.CanAccessHotelAsync(id, ct) || !(currentUser.IsSuperAdmin || currentUser.IsInRole(Common.Roles.HotelManager)))
        {
            throw new ForbiddenAccessException("Only SuperAdmin or that hotel's HotelManager can update hotel details.");
        }

        var hotel = await db.Hotels.FirstOrDefaultAsync(h => h.Id == id, ct)
            ?? throw new NotFoundException(nameof(Hotel), id);

        hotel.Name = request.Name;
        hotel.AddressLine1 = request.AddressLine1;
        hotel.AddressLine2 = request.AddressLine2;
        hotel.City = request.City;
        hotel.Pincode = request.Pincode;
        hotel.StateCode = request.StateCode;
        hotel.StateName = request.StateName;
        hotel.Gstin = request.Gstin;
        hotel.TimeZoneId = request.TimeZoneId;
        hotel.IsActive = request.IsActive;
        hotel.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    private static readonly System.Linq.Expressions.Expression<Func<Hotel, HotelDto>> ToDtoExpression = h => new HotelDto(
        h.Id, h.HotelGroupId, h.Code, h.Name, h.AddressLine1, h.AddressLine2, h.City, h.Pincode,
        h.StateCode, h.StateName, h.Gstin, h.TimeZoneId, h.BusinessDate, h.IsActive);
}
