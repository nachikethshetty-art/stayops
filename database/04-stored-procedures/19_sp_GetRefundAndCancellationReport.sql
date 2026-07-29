/*
    sp_GetRefundAndCancellationReport - cancellation/no-show detail with penalty and refund status
    for a hotel over a date range (filtered by cancellation business date).
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetRefundAndCancellationReport
    @HotelId UNIQUEIDENTIFIER,
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.Id AS CancellationId,
        c.ReservationId,
        res.ReservationNumber,
        c.TriggerType,
        c.CancelledAtUtc,
        c.HotelBusinessDateAtCancellation,
        c.StayGrossAmount,
        c.PenaltyAmount,
        c.PenaltyGstAmount,
        c.RefundDueAmount,
        rf.Id AS RefundId,
        rf.Status AS RefundStatus,
        rf.Amount AS RefundAmount,
        rf.CompletedAtUtc AS RefundCompletedAtUtc
    FROM dbo.Cancellations c
    JOIN dbo.Reservations res ON res.Id = c.ReservationId
    LEFT JOIN dbo.Refunds rf ON rf.CancellationId = c.Id
    WHERE res.HotelId = @HotelId
      AND c.HotelBusinessDateAtCancellation BETWEEN @FromDate AND @ToDate
    ORDER BY c.CancelledAtUtc DESC;

    SELECT
        COUNT(*) AS TotalCancellations,
        SUM(CASE WHEN c.TriggerType = 1 THEN 1 ELSE 0 END) AS TotalNoShows,
        SUM(c.PenaltyAmount + c.PenaltyGstAmount) AS TotalPenaltyCollected,
        SUM(c.RefundDueAmount) AS TotalRefundDue,
        SUM(CASE WHEN rf.Status = 3 THEN rf.Amount ELSE 0 END) AS TotalRefundsSucceeded,
        SUM(CASE WHEN rf.Status IN (0, 1, 2) THEN rf.Amount ELSE 0 END) AS TotalRefundsPending
    FROM dbo.Cancellations c
    JOIN dbo.Reservations res ON res.Id = c.ReservationId
    LEFT JOIN dbo.Refunds rf ON rf.CancellationId = c.Id
    WHERE res.HotelId = @HotelId
      AND c.HotelBusinessDateAtCancellation BETWEEN @FromDate AND @ToDate;
END;
GO
