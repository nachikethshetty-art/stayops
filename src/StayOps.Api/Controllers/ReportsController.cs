using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Common;
using StayOps.Application.Reports;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/v1/hotels/{hotelId:guid}/reports")]
[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager},{Roles.FinanceUser}")]
public class ReportsController(IReportService service) : ControllerBase
{
    [HttpGet("occupancy")]
    public async Task<ActionResult<IReadOnlyList<OccupancyReportRowDto>>> Occupancy(Guid hotelId, [FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, CancellationToken ct)
        => Ok(await service.GetOccupancyReportAsync(hotelId, fromDate, toDate, ct));

    [HttpGet("revenue-gst")]
    public async Task<ActionResult<IReadOnlyList<DailyRevenueReportRowDto>>> RevenueGst(Guid hotelId, [FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, CancellationToken ct)
        => Ok(await service.GetDailyRevenueAndGstReportAsync(hotelId, fromDate, toDate, ct));

    [HttpGet("refunds-cancellations")]
    public async Task<ActionResult<CancellationReportDto>> RefundsAndCancellations(Guid hotelId, [FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, CancellationToken ct)
        => Ok(await service.GetRefundAndCancellationReportAsync(hotelId, fromDate, toDate, ct));

    [HttpGet("corporate-receivables")]
    public async Task<ActionResult<IReadOnlyList<CorporateReceivableRowDto>>> CorporateReceivables(Guid hotelId, CancellationToken ct)
        => Ok(await service.GetCorporateReceivablesReportAsync(hotelId, ct));
}
