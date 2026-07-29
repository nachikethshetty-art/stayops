using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Application.Reservations;
using StayOps.Domain.Entities.Inventory;
using StayOps.Domain.Enums;

namespace StayOps.Infrastructure.Reservations;

internal class FolioSummaryRow
{
    public Guid FolioId { get; set; }
    public int FolioType { get; set; }
    public int FolioStatus { get; set; }
    public decimal Balance { get; set; }
}

public class CheckInCheckOutService(IDapperConnectionFactory connectionFactory, IApplicationDbContext db) : ICheckInCheckOutService
{
    public async Task<IReadOnlyList<StayFolioSummaryDto>> CheckInAsync(Guid reservationId, CheckInRequest request, Guid? userId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("ReservationId", reservationId);
        parameters.Add("RoomId", request.RoomId);
        parameters.Add("CheckedInByUserId", userId);

        try
        {
            var command = new CommandDefinition("sp_CheckInGuest", parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct);
            var rows = await connection.QueryAsync<FolioSummaryRow>(command);
            return rows.Select(ToDto).ToList();
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Class == 16)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    public async Task<IReadOnlyList<StayFolioSummaryDto>> CheckOutAsync(Guid reservationId, CheckOutRequest request, Guid? userId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("ReservationId", reservationId);
        parameters.Add("CheckedOutByUserId", userId);
        parameters.Add("ForceCheckout", request.ForceCheckout);

        try
        {
            var command = new CommandDefinition("sp_CheckOutGuest", parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct);
            var rows = await connection.QueryAsync<FolioSummaryRow>(command);
            return rows.Select(ToDto).ToList();
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Class == 16)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    public async Task MoveRoomAsync(Guid reservationId, MoveRoomRequest request, Guid? userId, CancellationToken ct = default)
    {
        var reservation = await db.Reservations.FirstOrDefaultAsync(r => r.Id == reservationId, ct)
            ?? throw new NotFoundException("Reservation", reservationId);

        if (reservation.Status != ReservationStatus.CheckedIn)
        {
            throw new BusinessRuleException("Only a CheckedIn reservation can have its room moved.");
        }

        var currentAssignment = await db.ReservationRoomAssignments
            .FirstOrDefaultAsync(a => a.ReservationId == reservationId && a.UnassignedAtUtc == null, ct)
            ?? throw new BusinessRuleException("Reservation has no current room assignment to move from.");

        var newRoom = await db.Rooms.FirstOrDefaultAsync(r => r.Id == request.NewRoomId && r.HotelId == reservation.HotelId, ct)
            ?? throw new NotFoundException(nameof(Room), request.NewRoomId);

        if (newRoom.RoomTypeId != reservation.RoomTypeId)
        {
            throw new BusinessRuleException("The new room must be the same room type as the reservation.");
        }

        if (newRoom.Status != RoomStatus.Available)
        {
            throw new BusinessRuleException("The new room is not currently available.");
        }

        var oldRoom = await db.Rooms.FirstAsync(r => r.Id == currentAssignment.RoomId, ct);
        var now = DateTime.UtcNow;

        currentAssignment.UnassignedAtUtc = now;
        currentAssignment.MoveReason = request.Reason;

        db.ReservationRoomAssignments.Add(new Domain.Entities.Reservations.ReservationRoomAssignment
        {
            ReservationId = reservationId,
            RoomId = newRoom.Id,
            AssignedAtUtc = now
        });

        var oldFromStatus = oldRoom.Status;
        oldRoom.Status = RoomStatus.Dirty;
        oldRoom.UpdatedAtUtc = now;
        db.RoomStatusHistories.Add(new RoomStatusHistory { RoomId = oldRoom.Id, FromStatus = oldFromStatus, ToStatus = RoomStatus.Dirty, Reason = "Room move - vacated", ChangedByUserId = userId });

        var newFromStatus = newRoom.Status;
        newRoom.Status = RoomStatus.Occupied;
        newRoom.UpdatedAtUtc = now;
        db.RoomStatusHistories.Add(new RoomStatusHistory { RoomId = newRoom.Id, FromStatus = newFromStatus, ToStatus = RoomStatus.Occupied, Reason = "Room move - occupied", ChangedByUserId = userId });

        db.HousekeepingTasks.Add(new HousekeepingTask
        {
            HotelId = reservation.HotelId,
            RoomId = oldRoom.Id,
            TaskType = HousekeepingTaskType.CleanAfterCheckout,
            Status = HousekeepingTaskStatus.Pending,
            Notes = $"Room move - vacated for reservation {reservation.ReservationNumber}"
        });

        await db.SaveChangesAsync(ct);
    }

    private static StayFolioSummaryDto ToDto(FolioSummaryRow r) => new(r.FolioId, (FolioType)r.FolioType, (FolioStatus)r.FolioStatus, r.Balance);
}
