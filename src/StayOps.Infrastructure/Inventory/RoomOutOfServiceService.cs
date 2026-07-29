using System.Data;
using Dapper;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Application.Inventory;
using StayOps.Domain.Enums;

namespace StayOps.Infrastructure.Inventory;

internal class OosPeriodRow
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public int Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public DateTime? ReturnedToServiceAtUtc { get; set; }
}

public class RoomOutOfServiceService(IDapperConnectionFactory connectionFactory) : IRoomOutOfServiceService
{
    public async Task<RoomOutOfServicePeriodDto> SetOutOfOrderAsync(Guid hotelId, Guid roomId, SetRoomOutOfOrderRequest request, Guid? userId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();

        var roomHotelId = await connection.QuerySingleOrDefaultAsync<Guid?>(
            new CommandDefinition("SELECT HotelId FROM dbo.Rooms WHERE Id = @RoomId", new { RoomId = roomId }, cancellationToken: ct));
        if (roomHotelId != hotelId)
        {
            throw new NotFoundException("Room", roomId);
        }

        var parameters = new DynamicParameters();
        parameters.Add("RoomId", roomId);
        parameters.Add("Type", (int)request.Type);
        parameters.Add("StartDate", request.StartDate.ToDateTime(TimeOnly.MinValue), DbType.Date);
        parameters.Add("EndDate", request.EndDate.ToDateTime(TimeOnly.MinValue), DbType.Date);
        parameters.Add("Reason", request.Reason);
        parameters.Add("RequestedByUserId", userId);
        parameters.Add("ApprovedByUserId", userId);

        try
        {
            var command = new CommandDefinition("sp_SetRoomOutOfOrder", parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct);
            var row = await connection.QuerySingleAsync<OosPeriodRow>(command);
            return await ToDtoAsync(connection, row, ct);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Class == 16)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    public async Task<RoomOutOfServicePeriodDto> ReturnToServiceAsync(Guid hotelId, Guid periodId, Guid? userId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("PeriodId", periodId);
        parameters.Add("ReturnedByUserId", userId);

        try
        {
            var command = new CommandDefinition("sp_ReturnRoomToService", parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct);
            var row = await connection.QuerySingleAsync<OosPeriodRow>(command);
            return await ToDtoAsync(connection, row, ct);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Class == 16)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    public async Task<IReadOnlyList<RoomOutOfServicePeriodDto>> GetByHotelAsync(Guid hotelId, bool activeOnly, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = $"""
            SELECT ro.Id, ro.RoomId, r.RoomNumber, ro.Type, ro.StartDate, ro.EndDate, ro.Reason, ro.Status, ro.ApprovedAtUtc, ro.ReturnedToServiceAtUtc
            FROM dbo.RoomOutOfServicePeriods ro
            JOIN dbo.Rooms r ON r.Id = ro.RoomId
            WHERE r.HotelId = @HotelId {(activeOnly ? "AND ro.Status = 1" : "")}
            ORDER BY ro.StartDate DESC
            """;
        var rows = await connection.QueryAsync<OosPeriodRowWithRoomNumber>(new CommandDefinition(sql, new { HotelId = hotelId }, cancellationToken: ct));
        return rows.Select(r => new RoomOutOfServicePeriodDto(
            r.Id, r.RoomId, r.RoomNumber, (RoomOutOfServiceType)r.Type, DateOnly.FromDateTime(r.StartDate), DateOnly.FromDateTime(r.EndDate),
            r.Reason, (OutOfServiceStatus)r.Status, r.ApprovedAtUtc, r.ReturnedToServiceAtUtc)).ToList();
    }

    private static async Task<RoomOutOfServicePeriodDto> ToDtoAsync(IDbConnection connection, OosPeriodRow row, CancellationToken ct)
    {
        var roomNumber = await connection.QuerySingleAsync<string>(
            new CommandDefinition("SELECT RoomNumber FROM dbo.Rooms WHERE Id = @RoomId", new { row.RoomId }, cancellationToken: ct));

        return new RoomOutOfServicePeriodDto(
            row.Id, row.RoomId, roomNumber, (RoomOutOfServiceType)row.Type, DateOnly.FromDateTime(row.StartDate), DateOnly.FromDateTime(row.EndDate),
            row.Reason, (OutOfServiceStatus)row.Status, row.ApprovedAtUtc, row.ReturnedToServiceAtUtc);
    }

    private class OosPeriodRowWithRoomNumber : OosPeriodRow
    {
        public string RoomNumber { get; set; } = string.Empty;
    }
}
