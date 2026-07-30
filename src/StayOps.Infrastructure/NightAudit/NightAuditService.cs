using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Application.NightAudit;
using StayOps.Domain.Enums;

namespace StayOps.Infrastructure.NightAudit;

internal class NightAuditRunRow
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public DateTime BusinessDate { get; set; }
    public int Status { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public decimal TotalRoomRevenuePosted { get; set; }
    public decimal TotalTaxPosted { get; set; }
    public int StaysProcessed { get; set; }
    public int NoShowCount { get; set; }
    public int ExceptionCount { get; set; }
}

public class NightAuditService(IDapperConnectionFactory connectionFactory, IApplicationDbContext db) : Application.NightAudit.INightAuditService
{
    /// <summary>Night audit is a once-a-night operation - even a hotel that's legitimately catching up on a backlog shouldn't be able to fire runs back-to-back.</summary>
    private static readonly TimeSpan MinIntervalBetweenRuns = TimeSpan.FromHours(5);

    public async Task<NightAuditRunDto> RunAsync(Guid hotelId, Guid? triggeredByUserId, CancellationToken ct = default)
    {
        var hotel = await db.Hotels.Where(h => h.Id == hotelId)
            .Select(h => new { h.TimeZoneId, h.BusinessDate })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("Hotel", hotelId);

        var todayLocal = ResolveTodayLocal(hotel.TimeZoneId);
        if (hotel.BusinessDate > todayLocal)
        {
            throw new BusinessRuleException(
                $"Night audit cannot be run for business date {hotel.BusinessDate:yyyy-MM-dd} - that date has not started yet in the hotel's local time (today is {todayLocal:yyyy-MM-dd}).");
        }

        using var connection = connectionFactory.CreateConnection();

        var lastCompletedAtUtc = await connection.QuerySingleOrDefaultAsync<DateTime?>(
            new CommandDefinition(
                "SELECT MAX(CompletedAtUtc) FROM dbo.NightAuditRuns WHERE HotelId = @HotelId AND Status = 1 /* Completed */",
                new { HotelId = hotelId }, cancellationToken: ct));

        if (lastCompletedAtUtc is { } lastRunAtUtc && DateTime.UtcNow - lastRunAtUtc < MinIntervalBetweenRuns)
        {
            var retryAfterUtc = lastRunAtUtc + MinIntervalBetweenRuns;
            throw new BusinessRuleException(
                $"Night audit last completed at {lastRunAtUtc:yyyy-MM-dd HH:mm} UTC. It can be run again after {retryAfterUtc:yyyy-MM-dd HH:mm} UTC.");
        }

        var parameters = new DynamicParameters();
        parameters.Add("HotelId", hotelId);
        parameters.Add("TriggeredByUserId", triggeredByUserId);

        try
        {
            var command = new CommandDefinition("sp_RunNightAudit", parameters, commandType: CommandType.StoredProcedure, commandTimeout: 180, cancellationToken: ct);
            var row = await connection.QuerySingleAsync<NightAuditRunRow>(command);
            return ToDto(row);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Class == 16)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    private static DateOnly ResolveTodayLocal(string timeZoneId)
    {
        TimeZoneInfo tz;
        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            tz = TimeZoneInfo.Utc;
        }

        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));
    }

    public async Task<IReadOnlyList<NightAuditRunDto>> GetHistoryAsync(Guid hotelId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM dbo.NightAuditRuns WHERE HotelId = @HotelId ORDER BY BusinessDate DESC";
        var rows = await connection.QueryAsync<NightAuditRunRow>(new CommandDefinition(sql, new { HotelId = hotelId }, cancellationToken: ct));
        return rows.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<NightAuditExceptionDto>> GetExceptionsAsync(Guid runId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT Id, ReservationId, ExceptionType, Message, CreatedAtUtc FROM dbo.NightAuditExceptions WHERE NightAuditRunId = @RunId ORDER BY CreatedAtUtc";
        var rows = await connection.QueryAsync<NightAuditExceptionDto>(new CommandDefinition(sql, new { RunId = runId }, cancellationToken: ct));
        return rows.ToList();
    }

    private static NightAuditRunDto ToDto(NightAuditRunRow r) => new(
        r.Id, r.HotelId, DateOnly.FromDateTime(r.BusinessDate), (NightAuditRunStatus)r.Status, r.StartedAtUtc, r.CompletedAtUtc,
        r.TotalRoomRevenuePosted, r.TotalTaxPosted, r.StaysProcessed, r.NoShowCount, r.ExceptionCount);
}
