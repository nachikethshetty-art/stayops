/*
    sp_SearchAvailableRoomTypes - the single availability search used by BOTH the online booking
    demo screen and the reception availability workspace. Returns one row per room type that still
    has sellable inventory for the whole requested date range, with its resolved rate.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_SearchAvailableRoomTypes
    @HotelId UNIQUEIDENTIFIER,
    @CheckInDate DATE,
    @CheckOutDate DATE,
    @Adults INT = 1,
    @Children INT = 0,
    @RequestedRatePlanId UNIQUEIDENTIFIER = NULL,
    @CompanyId UNIQUEIDENTIFIER = NULL,
    @TravelAgentId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @CheckOutDate <= @CheckInDate
    BEGIN
        RAISERROR('CheckOutDate must be after CheckInDate.', 16, 1);
        RETURN;
    END

    DECLARE @Occupancy INT = CASE WHEN (@Adults + @Children) < 1 THEN 1 ELSE (@Adults + @Children) END;

    ;WITH Nights AS (
        SELECT DATEADD(DAY, value, @CheckInDate) AS StayDate
        FROM GENERATE_SERIES(0, DATEDIFF(DAY, @CheckInDate, @CheckOutDate) - 1)
    ),
    Search AS (
        SELECT
            rt.Id AS RoomTypeId,
            rt.Name AS RoomTypeName,
            rt.BaseOccupancy,
            rt.MaxOccupancy,
            SUM(rate.Rate) AS TotalRoomRateExclGst,
            AVG(rate.Rate) AS AverageNightlyRate,
            MAX(rate.RatePlanId) AS RatePlanId,
            MAX(rate.RatePlanName) AS RatePlanName,
            MAX(rate.MealPlan) AS MealPlan,
            MAX(rate.RateSource) AS RateSource
        FROM dbo.RoomTypes rt
        CROSS JOIN Nights n
        CROSS APPLY dbo.fn_ResolveNightlyRate(@HotelId, rt.Id, n.StayDate, @Occupancy, @RequestedRatePlanId, @CompanyId, @TravelAgentId) rate
        WHERE rt.HotelId = @HotelId AND rt.IsActive = 1 AND @Occupancy <= rt.MaxOccupancy
        GROUP BY rt.Id, rt.Name, rt.BaseOccupancy, rt.MaxOccupancy
    )
    SELECT s.*, dbo.fn_RoomTypeAvailableCount(@HotelId, s.RoomTypeId, @CheckInDate, @CheckOutDate) AS AvailableCount
    INTO #Results
    FROM Search s;

    SELECT * FROM #Results WHERE AvailableCount > 0 ORDER BY RoomTypeName;

    DROP TABLE #Results;
END;
GO
