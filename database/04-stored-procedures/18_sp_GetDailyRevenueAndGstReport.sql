/*
    sp_GetDailyRevenueAndGstReport - per business-date revenue and GST breakdown from posted folio
    charges (RoomCharge/Incidental) at this hotel, over a date range.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetDailyRevenueAndGstReport
    @HotelId UNIQUEIDENTIFIER,
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ft.BusinessDate,
        SUM(CASE WHEN ft.Type = 0 THEN ft.Amount ELSE 0 END) AS RoomRevenue,
        SUM(CASE WHEN ft.Type = 1 THEN ft.Amount ELSE 0 END) AS IncidentalRevenue,
        SUM(ft.Amount) AS TotalTaxableRevenue,
        SUM(ft.GstAmount) AS TotalGst,
        SUM(ft.TotalAmount) AS TotalRevenueInclGst
    FROM dbo.FolioTransactions ft
    JOIN dbo.Folios f ON f.Id = ft.FolioId
    JOIN dbo.Reservations r ON r.Id = f.ReservationId
    WHERE r.HotelId = @HotelId
      AND ft.Type IN (0, 1) /* RoomCharge, Incidental */
      AND ft.BusinessDate BETWEEN @FromDate AND @ToDate
    GROUP BY ft.BusinessDate
    ORDER BY ft.BusinessDate;
END;
GO
