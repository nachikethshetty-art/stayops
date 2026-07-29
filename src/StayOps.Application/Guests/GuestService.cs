using Microsoft.EntityFrameworkCore;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Application.Common.Models;
using StayOps.Domain.Entities.Guests;

namespace StayOps.Application.Guests;

public class GuestService(IApplicationDbContext db) : IGuestService
{
    public async Task<PagedResult<GuestDto>> SearchAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = db.Guests.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(g =>
                g.FirstName.Contains(term) || g.LastName.Contains(term) ||
                g.Phone.Contains(term) || g.Email.Contains(term));
        }

        query = request.SortBy switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(g => g.LastName) : query.OrderBy(g => g.LastName),
            _ => request.SortDescending ? query.OrderByDescending(g => g.CreatedAtUtc) : query.OrderBy(g => g.CreatedAtUtc)
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ToDtoExpression)
            .ToListAsync(ct);

        return new PagedResult<GuestDto> { Items = items, Page = request.Page, PageSize = request.PageSize, TotalCount = total };
    }

    public async Task<GuestDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Guests.Where(g => g.Id == id).Select(ToDtoExpression).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Guest), id);
    }

    public async Task<GuestDto> CreateAsync(CreateGuestRequest request, CancellationToken ct = default)
    {
        var guest = new Guest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            IdProofType = request.IdProofType,
            IdProofNumber = request.IdProofNumber,
            AddressLine1 = request.AddressLine1,
            City = request.City,
            StateCode = request.StateCode,
            Pincode = request.Pincode,
            Gstin = request.Gstin
        };
        db.Guests.Add(guest);
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(guest.Id, ct);
    }

    public async Task<GuestDto> UpdateAsync(Guid id, UpdateGuestRequest request, CancellationToken ct = default)
    {
        var guest = await db.Guests.FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new NotFoundException(nameof(Guest), id);

        guest.FirstName = request.FirstName;
        guest.LastName = request.LastName;
        guest.Email = request.Email;
        guest.Phone = request.Phone;
        guest.IdProofType = request.IdProofType;
        guest.IdProofNumber = request.IdProofNumber;
        guest.AddressLine1 = request.AddressLine1;
        guest.City = request.City;
        guest.StateCode = request.StateCode;
        guest.Pincode = request.Pincode;
        guest.Gstin = request.Gstin;
        guest.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    private static readonly System.Linq.Expressions.Expression<Func<Guest, GuestDto>> ToDtoExpression = g => new GuestDto(
        g.Id, g.FirstName, g.LastName, g.Email, g.Phone, g.IdProofType, g.IdProofNumber, g.AddressLine1, g.City, g.StateCode, g.Pincode, g.Gstin);
}
