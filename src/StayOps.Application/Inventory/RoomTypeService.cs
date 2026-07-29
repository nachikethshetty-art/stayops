using Microsoft.EntityFrameworkCore;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Domain.Entities.Inventory;

namespace StayOps.Application.Inventory;

public class RoomTypeService(IApplicationDbContext db) : IRoomTypeService
{
    public async Task<IReadOnlyList<RoomTypeDto>> GetByHotelAsync(Guid hotelId, CancellationToken ct = default)
    {
        return await db.RoomTypes.Where(rt => rt.HotelId == hotelId).Select(ToDtoExpression).ToListAsync(ct);
    }

    public async Task<RoomTypeDto> GetByIdAsync(Guid hotelId, Guid id, CancellationToken ct = default)
    {
        return await db.RoomTypes.Where(rt => rt.HotelId == hotelId && rt.Id == id).Select(ToDtoExpression).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(RoomType), id);
    }

    public async Task<RoomTypeDto> CreateAsync(Guid hotelId, CreateRoomTypeRequest request, CancellationToken ct = default)
    {
        if (await db.RoomTypes.AnyAsync(rt => rt.HotelId == hotelId && rt.Code == request.Code, ct))
        {
            throw new ConflictException($"Room type code '{request.Code}' already exists for this hotel.");
        }

        var roomType = new RoomType
        {
            HotelId = hotelId,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            BaseOccupancy = request.BaseOccupancy,
            MaxOccupancy = request.MaxOccupancy,
            MaxChildren = request.MaxChildren
        };
        db.RoomTypes.Add(roomType);
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(hotelId, roomType.Id, ct);
    }

    public async Task<RoomTypeDto> UpdateAsync(Guid hotelId, Guid id, UpdateRoomTypeRequest request, CancellationToken ct = default)
    {
        var roomType = await db.RoomTypes.FirstOrDefaultAsync(rt => rt.HotelId == hotelId && rt.Id == id, ct)
            ?? throw new NotFoundException(nameof(RoomType), id);

        roomType.Name = request.Name;
        roomType.Description = request.Description;
        roomType.BaseOccupancy = request.BaseOccupancy;
        roomType.MaxOccupancy = request.MaxOccupancy;
        roomType.MaxChildren = request.MaxChildren;
        roomType.IsActive = request.IsActive;
        roomType.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(hotelId, id, ct);
    }

    private static readonly System.Linq.Expressions.Expression<Func<RoomType, RoomTypeDto>> ToDtoExpression = rt => new RoomTypeDto(
        rt.Id, rt.HotelId, rt.Code, rt.Name, rt.Description, rt.BaseOccupancy, rt.MaxOccupancy, rt.MaxChildren, rt.IsActive, rt.Rooms.Count);
}
