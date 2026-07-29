using System.Data;
using Dapper;
using StayOps.Application.Common.Interfaces;
using StayOps.Application.Reports;
using StayOps.Domain.Enums;

namespace StayOps.Infrastructure.Reports;

internal class OccupancyRow
{
    public DateTime ReportDate { get; set; }
    public int TotalActiveRooms { get; set; }
    public int OutOfOrderRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public decimal OccupancyPercent { get; set; }
}

internal class DailyRevenueRow
{
    public DateTime BusinessDate { get; set; }
    public decimal RoomRevenue { get; set; }
    public decimal IncidentalRevenue { get; set; }
    public decimal TotalTaxableRevenue { get; set; }
    public decimal TotalGst { get; set; }
    public decimal TotalRevenueInclGst { get; set; }
}

internal class CancellationRow
{
    public Guid CancellationId { get; set; }
    public Guid ReservationId { get; set; }
    public string ReservationNumber { get; set; } = string.Empty;
    public int TriggerType { get; set; }
    public DateTime CancelledAtUtc { get; set; }
    public DateTime HotelBusinessDateAtCancellation { get; set; }
    public decimal StayGrossAmount { get; set; }
    public decimal PenaltyAmount { get; set; }
    public decimal PenaltyGstAmount { get; set; }
    public decimal RefundDueAmount { get; set; }
    public Guid? RefundId { get; set; }
    public int? RefundStatus { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime? RefundCompletedAtUtc { get; set; }
}

public class ReportService(IDapperConnectionFactory connectionFactory) : IReportService
{
    public async Task<IReadOnlyList<OccupancyReportRowDto>> GetOccupancyReportAsync(Guid hotelId, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("HotelId", hotelId);
        parameters.Add("FromDate", fromDate.ToDateTime(TimeOnly.MinValue), DbType.Date);
        parameters.Add("ToDate", toDate.ToDateTime(TimeOnly.MinValue), DbType.Date);

        var command = new CommandDefinition("sp_GetOccupancyReport", parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct);
        var rows = await connection.QueryAsync<OccupancyRow>(command);
        return rows.Select(r => new OccupancyReportRowDto(
            DateOnly.FromDateTime(r.ReportDate), r.TotalActiveRooms, r.OutOfOrderRooms, r.OccupiedRooms, r.OccupancyPercent)).ToList();
    }

    public async Task<IReadOnlyList<DailyRevenueReportRowDto>> GetDailyRevenueAndGstReportAsync(Guid hotelId, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("HotelId", hotelId);
        parameters.Add("FromDate", fromDate.ToDateTime(TimeOnly.MinValue), DbType.Date);
        parameters.Add("ToDate", toDate.ToDateTime(TimeOnly.MinValue), DbType.Date);

        var command = new CommandDefinition("sp_GetDailyRevenueAndGstReport", parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct);
        var rows = await connection.QueryAsync<DailyRevenueRow>(command);
        return rows.Select(r => new DailyRevenueReportRowDto(
            DateOnly.FromDateTime(r.BusinessDate), r.RoomRevenue, r.IncidentalRevenue, r.TotalTaxableRevenue, r.TotalGst, r.TotalRevenueInclGst)).ToList();
    }

    public async Task<CancellationReportDto> GetRefundAndCancellationReportAsync(Guid hotelId, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("HotelId", hotelId);
        parameters.Add("FromDate", fromDate.ToDateTime(TimeOnly.MinValue), DbType.Date);
        parameters.Add("ToDate", toDate.ToDateTime(TimeOnly.MinValue), DbType.Date);

        var command = new CommandDefinition("sp_GetRefundAndCancellationReport", parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct);
        using var multi = await connection.QueryMultipleAsync(command);

        var rows = (await multi.ReadAsync<CancellationRow>()).Select(r => new CancellationReportRowDto(
            r.CancellationId, r.ReservationId, r.ReservationNumber, (CancellationTriggerType)r.TriggerType, r.CancelledAtUtc,
            DateOnly.FromDateTime(r.HotelBusinessDateAtCancellation), r.StayGrossAmount, r.PenaltyAmount, r.PenaltyGstAmount, r.RefundDueAmount,
            r.RefundId, r.RefundStatus.HasValue ? (RefundStatus)r.RefundStatus.Value : null, r.RefundAmount, r.RefundCompletedAtUtc)).ToList();

        var summary = await multi.ReadSingleAsync<CancellationReportSummaryDto>();

        return new CancellationReportDto(rows, summary);
    }

    public async Task<IReadOnlyList<CorporateReceivableRowDto>> GetCorporateReceivablesReportAsync(Guid hotelId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition("sp_GetCorporateReceivablesReport", new { HotelId = hotelId }, commandType: CommandType.StoredProcedure, cancellationToken: ct);
        var rows = await connection.QueryAsync<CorporateReceivableRowDto>(command);
        return rows.ToList();
    }
}
