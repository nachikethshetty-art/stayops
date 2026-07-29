using Microsoft.EntityFrameworkCore;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Domain.Entities.Inventory;
using StayOps.Domain.Enums;

namespace StayOps.Application.Housekeeping;

public class HousekeepingService(IApplicationDbContext db, ICurrentUserService currentUser) : IHousekeepingService
{
    public async Task<IReadOnlyList<HousekeepingTaskDto>> GetByHotelAsync(Guid hotelId, HousekeepingTaskStatus? status, CancellationToken ct = default)
    {
        var query = db.HousekeepingTasks.Where(t => t.HotelId == hotelId);
        if (status is not null) query = query.Where(t => t.Status == status);

        return await query
            .OrderBy(t => t.Status).ThenBy(t => t.CreatedAtUtc)
            .Select(t => new HousekeepingTaskDto(t.Id, t.HotelId, t.RoomId, t.Room!.RoomNumber, t.TaskType, t.Status, t.AssignedToUserId, t.Notes, t.CreatedAtUtc, t.CompletedAtUtc))
            .ToListAsync(ct);
    }

    public async Task<HousekeepingTaskDto> CreateAsync(Guid hotelId, CreateHousekeepingTaskRequest request, CancellationToken ct = default)
    {
        var room = await db.Rooms.FirstOrDefaultAsync(r => r.HotelId == hotelId && r.Id == request.RoomId, ct)
            ?? throw new NotFoundException(nameof(Room), request.RoomId);

        var task = new HousekeepingTask
        {
            HotelId = hotelId,
            RoomId = room.Id,
            TaskType = request.TaskType,
            Notes = request.Notes,
            AssignedToUserId = request.AssignedToUserId,
            Status = HousekeepingTaskStatus.Pending
        };
        db.HousekeepingTasks.Add(task);
        await db.SaveChangesAsync(ct);

        return new HousekeepingTaskDto(task.Id, task.HotelId, task.RoomId, room.RoomNumber, task.TaskType, task.Status, task.AssignedToUserId, task.Notes, task.CreatedAtUtc, task.CompletedAtUtc);
    }

    public async Task<HousekeepingTaskDto> UpdateStatusAsync(Guid hotelId, Guid taskId, UpdateHousekeepingTaskStatusRequest request, CancellationToken ct = default)
    {
        var task = await db.HousekeepingTasks.Include(t => t.Room).FirstOrDefaultAsync(t => t.HotelId == hotelId && t.Id == taskId, ct)
            ?? throw new NotFoundException(nameof(HousekeepingTask), taskId);

        task.Status = request.Status;
        if (request.AssignedToUserId is not null) task.AssignedToUserId = request.AssignedToUserId;
        if (request.Notes is not null) task.Notes = request.Notes;
        task.UpdatedAtUtc = DateTime.UtcNow;

        if (request.Status == HousekeepingTaskStatus.InProgress && task.StartedAtUtc is null)
        {
            task.StartedAtUtc = DateTime.UtcNow;
        }

        if (request.Status == HousekeepingTaskStatus.Completed)
        {
            task.CompletedAtUtc = DateTime.UtcNow;

            // Closing the loop: a completed post-checkout clean brings the room back to sellable Available status.
            if (task.TaskType == HousekeepingTaskType.CleanAfterCheckout && task.Room is not null && task.Room.Status == RoomStatus.Dirty)
            {
                var fromStatus = task.Room.Status;
                task.Room.Status = RoomStatus.Available;
                task.Room.UpdatedAtUtc = DateTime.UtcNow;
                db.RoomStatusHistories.Add(new RoomStatusHistory
                {
                    RoomId = task.Room.Id,
                    FromStatus = fromStatus,
                    ToStatus = RoomStatus.Available,
                    Reason = "Housekeeping task completed",
                    ChangedByUserId = currentUser.UserId
                });
            }
        }

        await db.SaveChangesAsync(ct);
        return new HousekeepingTaskDto(task.Id, task.HotelId, task.RoomId, task.Room!.RoomNumber, task.TaskType, task.Status, task.AssignedToUserId, task.Notes, task.CreatedAtUtc, task.CompletedAtUtc);
    }
}
