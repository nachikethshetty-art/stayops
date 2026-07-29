using Microsoft.EntityFrameworkCore;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Domain.Entities.Inventory;
using StayOps.Domain.Enums;

namespace StayOps.Application.Inventory;

public class RoomService(IApplicationDbContext db, ICurrentUserService currentUser) : IRoomService
{
    public async Task<IReadOnlyList<RoomDto>> GetByHotelAsync(Guid hotelId, RoomStatus? status, Guid? roomTypeId, CancellationToken ct = default)
    {
        var query = db.Rooms.Where(r => r.HotelId == hotelId);
        if (status is not null) query = query.Where(r => r.Status == status);
        if (roomTypeId is not null) query = query.Where(r => r.RoomTypeId == roomTypeId);

        return await query
            .OrderBy(r => r.RoomNumber)
            .Select(r => new RoomDto(r.Id, r.HotelId, r.RoomTypeId, r.RoomType!.Name, r.RoomNumber, r.Floor, r.Status, r.IsActive))
            .ToListAsync(ct);
    }

    public async Task<RoomDto> GetByIdAsync(Guid hotelId, Guid id, CancellationToken ct = default)
    {
        return await db.Rooms.Where(r => r.HotelId == hotelId && r.Id == id)
            .Select(r => new RoomDto(r.Id, r.HotelId, r.RoomTypeId, r.RoomType!.Name, r.RoomNumber, r.Floor, r.Status, r.IsActive))
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Room), id);
    }

    public async Task<RoomDto> CreateAsync(Guid hotelId, CreateRoomRequest request, CancellationToken ct = default)
    {
        var roomTypeExists = await db.RoomTypes.AnyAsync(rt => rt.Id == request.RoomTypeId && rt.HotelId == hotelId, ct);
        if (!roomTypeExists)
        {
            throw new NotFoundException(nameof(RoomType), request.RoomTypeId);
        }

        if (await db.Rooms.AnyAsync(r => r.HotelId == hotelId && r.RoomNumber == request.RoomNumber, ct))
        {
            throw new ConflictException($"Room number '{request.RoomNumber}' already exists at this hotel.");
        }

        var room = new Room
        {
            HotelId = hotelId,
            RoomTypeId = request.RoomTypeId,
            RoomNumber = request.RoomNumber,
            Floor = request.Floor,
            Status = RoomStatus.Available
        };
        db.Rooms.Add(room);
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(hotelId, room.Id, ct);
    }

    public async Task<RoomDto> UpdateAsync(Guid hotelId, Guid id, UpdateRoomRequest request, CancellationToken ct = default)
    {
        var room = await db.Rooms.FirstOrDefaultAsync(r => r.HotelId == hotelId && r.Id == id, ct)
            ?? throw new NotFoundException(nameof(Room), id);

        room.RoomTypeId = request.RoomTypeId;
        room.RoomNumber = request.RoomNumber;
        room.Floor = request.Floor;
        room.IsActive = request.IsActive;
        room.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(hotelId, id, ct);
    }

    public async Task<RoomDto> ChangeStatusAsync(Guid hotelId, Guid id, ChangeRoomStatusRequest request, CancellationToken ct = default)
    {
        if (request.NewStatus is RoomStatus.OutOfOrder or RoomStatus.OutOfService)
        {
            throw new BusinessRuleException("Use the OOO/OOS request-and-approve workflow to take a room out of service, not this endpoint.");
        }

        var room = await db.Rooms.FirstOrDefaultAsync(r => r.HotelId == hotelId && r.Id == id, ct)
            ?? throw new NotFoundException(nameof(Room), id);

        if (room.Status is RoomStatus.OutOfOrder or RoomStatus.OutOfService)
        {
            throw new BusinessRuleException("Room is currently OOO/OOS - return it to service before changing its operational status.");
        }

        var fromStatus = room.Status;
        room.Status = request.NewStatus;
        room.UpdatedAtUtc = DateTime.UtcNow;

        db.RoomStatusHistories.Add(new RoomStatusHistory
        {
            RoomId = room.Id,
            FromStatus = fromStatus,
            ToStatus = request.NewStatus,
            Reason = request.Reason,
            ChangedByUserId = currentUser.UserId
        });

        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(hotelId, id, ct);
    }
}
