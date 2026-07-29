using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Application.Reservations;
using StayOps.Domain.Enums;

namespace StayOps.Infrastructure.Reservations;

internal class CancellationRow
{
    public Guid Id { get; set; }
    public Guid ReservationId { get; set; }
    public int TriggerType { get; set; }
    public DateTime CancelledAtUtc { get; set; }
    public DateTime HotelBusinessDateAtCancellation { get; set; }
    public int HoursBeforeCheckIn { get; set; }
    public decimal StayGrossAmount { get; set; }
    public decimal PenaltyAmount { get; set; }
    public decimal PenaltyGstAmount { get; set; }
    public decimal RefundDueAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid? RefundId { get; set; }
    public int? RefundStatus { get; set; }
}

public class CancellationService(IDapperConnectionFactory connectionFactory, IApplicationDbContext db) : ICancellationService
{
    public async Task<CancellationDto> CancelAsync(Guid reservationId, CancelReservationRequest request, Guid? cancelledByUserId, CancellationToken ct = default)
    {
        var (hotelId, checkInDate, timeZoneId, businessDate) = await LoadContextAsync(reservationId, ct);

        var hoursBeforeCheckIn = ComputeHoursBeforeCheckIn(checkInDate, timeZoneId);

        return await ExecuteCancelAsync(reservationId, triggerType: 0, hoursBeforeCheckIn, businessDate, request.Reason, cancelledByUserId, ct);
    }

    public async Task<CancellationDto> MarkNoShowAsync(Guid reservationId, string? reason, Guid? triggeredByUserId, CancellationToken ct = default)
    {
        var (_, _, _, businessDate) = await LoadContextAsync(reservationId, ct);
        return await ExecuteCancelAsync(reservationId, triggerType: 1, hoursBeforeCheckIn: null, businessDate, reason, triggeredByUserId, ct);
    }

    public async Task<CancellationDto?> GetByReservationIdAsync(Guid reservationId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """
            SELECT c.*, r.Id AS RefundId, r.Status AS RefundStatus
            FROM dbo.Cancellations c
            LEFT JOIN dbo.Refunds r ON r.CancellationId = c.Id
            WHERE c.ReservationId = @ReservationId
            """;
        var row = await connection.QuerySingleOrDefaultAsync<CancellationRow>(
            new CommandDefinition(sql, new { ReservationId = reservationId }, cancellationToken: ct));
        return row is null ? null : ToDto(row);
    }

    private async Task<(Guid hotelId, DateOnly checkInDate, string timeZoneId, DateOnly businessDate)> LoadContextAsync(Guid reservationId, CancellationToken ct)
    {
        var reservation = await db.Reservations.Where(r => r.Id == reservationId)
            .Select(r => new { r.HotelId, r.CheckInDate })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("Reservation", reservationId);

        var hotel = await db.Hotels.Where(h => h.Id == reservation.HotelId)
            .Select(h => new { h.TimeZoneId, h.BusinessDate })
            .FirstAsync(ct);

        return (reservation.HotelId, reservation.CheckInDate, hotel.TimeZoneId, hotel.BusinessDate);
    }

    /// <summary>
    /// Hours between "now" in hotel-local time and local midnight of the check-in date - this
    /// demo's documented stand-in for a formal check-in time (see sp_CancelReservation header).
    /// </summary>
    private static int ComputeHoursBeforeCheckIn(DateOnly checkInDate, string timeZoneId)
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

        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        var checkInLocalMidnight = checkInDate.ToDateTime(TimeOnly.MinValue);

        return (int)Math.Floor((checkInLocalMidnight - nowLocal).TotalHours);
    }

    private async Task<CancellationDto> ExecuteCancelAsync(
        Guid reservationId, int triggerType, int? hoursBeforeCheckIn, DateOnly businessDate, string? reason, Guid? userId, CancellationToken ct)
    {
        using var connection = connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("ReservationId", reservationId);
        parameters.Add("TriggerType", triggerType);
        parameters.Add("HoursBeforeCheckIn", hoursBeforeCheckIn);
        parameters.Add("BusinessDate", businessDate.ToDateTime(TimeOnly.MinValue), DbType.Date);
        parameters.Add("Reason", reason);
        parameters.Add("CancelledByUserId", userId);

        try
        {
            var command = new CommandDefinition("sp_CancelReservation", parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct);
            var row = await connection.QuerySingleAsync<CancellationRow>(command);
            return ToDto(row);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Class == 16)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    private static CancellationDto ToDto(CancellationRow r) => new(
        r.Id, r.ReservationId, (CancellationTriggerType)r.TriggerType, r.CancelledAtUtc,
        DateOnly.FromDateTime(r.HotelBusinessDateAtCancellation), r.HoursBeforeCheckIn,
        r.StayGrossAmount, r.PenaltyAmount, r.PenaltyGstAmount, r.RefundDueAmount, r.Reason,
        r.RefundId, r.RefundStatus.HasValue ? (RefundStatus)r.RefundStatus.Value : null);
}
