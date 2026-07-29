namespace StayOps.Application.Reports;

public interface IReportService
{
    Task<IReadOnlyList<OccupancyReportRowDto>> GetOccupancyReportAsync(Guid hotelId, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default);
    Task<IReadOnlyList<DailyRevenueReportRowDto>> GetDailyRevenueAndGstReportAsync(Guid hotelId, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default);
    Task<CancellationReportDto> GetRefundAndCancellationReportAsync(Guid hotelId, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default);
    Task<IReadOnlyList<CorporateReceivableRowDto>> GetCorporateReceivablesReportAsync(Guid hotelId, CancellationToken ct = default);
}
