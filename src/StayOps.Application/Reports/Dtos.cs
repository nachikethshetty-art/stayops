using StayOps.Domain.Enums;

namespace StayOps.Application.Reports;

public record OccupancyReportRowDto(DateOnly ReportDate, int TotalActiveRooms, int OutOfOrderRooms, int OccupiedRooms, decimal OccupancyPercent);

public record DailyRevenueReportRowDto(DateOnly BusinessDate, decimal RoomRevenue, decimal IncidentalRevenue, decimal TotalTaxableRevenue, decimal TotalGst, decimal TotalRevenueInclGst);

public record CancellationReportRowDto(
    Guid CancellationId, Guid ReservationId, string ReservationNumber, CancellationTriggerType TriggerType, DateTime CancelledAtUtc,
    DateOnly HotelBusinessDateAtCancellation, decimal StayGrossAmount, decimal PenaltyAmount, decimal PenaltyGstAmount, decimal RefundDueAmount,
    Guid? RefundId, RefundStatus? RefundStatus, decimal? RefundAmount, DateTime? RefundCompletedAtUtc);

public record CancellationReportSummaryDto(
    int TotalCancellations, int TotalNoShows, decimal TotalPenaltyCollected, decimal TotalRefundDue, decimal TotalRefundsSucceeded, decimal TotalRefundsPending);

public record CancellationReportDto(IReadOnlyList<CancellationReportRowDto> Rows, CancellationReportSummaryDto Summary);

public record CorporateReceivableRowDto(Guid CompanyId, string CompanyName, string Gstin, decimal CreditLimit, int OpenFolioCount, decimal TotalOutstandingBalance);
