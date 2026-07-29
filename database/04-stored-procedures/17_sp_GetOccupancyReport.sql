/*
    sp_GetOccupancyReport - per-night occupancy for a date range.
    Occupancy % = Occupied Rooms / (Total Active Rooms - OOO Rooms) x 100.
    "Occupied" = rooms booked by stays that actually happened (CheckedIn or CheckedOut), not
    merely Confirmed, so a historical date reflects reality rather than bookings-on-the-books.
    OOO rooms are excluded from the denominator; OOS rooms remain in it (demo policy, see README).
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetOccupancyReport
    @HotelId UNIQUEIDENTIFIER,
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalActiveRooms INT = (SELECT COUNT(*) FROM dbo.Rooms WHERE HotelId = @HotelId AND IsActive = 1);

    ;WITH Nights AS (
        SELECT DATEADD(DAY, value, @FromDate) AS ReportDate
        FROM GENERATE_SERIES(0, DATEDIFF(DAY, @FromDate, @ToDate))
    )
    SELECT
        n.ReportDate,
        @TotalActiveRooms AS TotalActiveRooms,
        (
            SELECT COUNT(DISTINCT ro.RoomId) FROM dbo.RoomOutOfServicePeriods ro
            JOIN dbo.Rooms rm ON rm.Id = ro.RoomId
            WHERE rm.HotelId = @HotelId AND ro.Type = 0 /* OutOfOrder */ AND ro.Status = 1 /* Approved */
              AND n.ReportDate >= ro.StartDate AND n.ReportDate < ro.EndDate
        ) AS OutOfOrderRooms,
        ISNULL((
            SELECT SUM(r.RoomsBooked) FROM dbo.Reservations r
            WHERE r.HotelId = @HotelId AND r.Status IN (2, 3) /* CheckedIn, CheckedOut */
              AND n.ReportDate >= r.CheckInDate AND n.ReportDate < r.CheckOutDate
        ), 0) AS OccupiedRooms
    INTO #Occ
    FROM Nights n;

    SELECT ReportDate, TotalActiveRooms, OutOfOrderRooms, OccupiedRooms,
           CASE WHEN (TotalActiveRooms - OutOfOrderRooms) <= 0 THEN 0
                ELSE ROUND(100.0 * OccupiedRooms / (TotalActiveRooms - OutOfOrderRooms), 2)
           END AS OccupancyPercent
    FROM #Occ
    ORDER BY ReportDate;

    DROP TABLE #Occ;
END;
GO
