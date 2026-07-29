/*
    Shared helper functions used by both the online-booking and reception-booking code paths so
    there is exactly one inventory-counting and rate-resolution implementation in the database -
    per the README requirement that online and reception booking "must never have separate
    availability counters."
*/

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.sequences WHERE name = 'ReservationNumberSeq')
    EXEC('CREATE SEQUENCE dbo.ReservationNumberSeq AS BIGINT START WITH 1000 INCREMENT BY 1;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.sequences WHERE name = 'InvoiceNumberSeq')
    EXEC('CREATE SEQUENCE dbo.InvoiceNumberSeq AS BIGINT START WITH 1000 INCREMENT BY 1;');
GO

-----------------------------------------------------------------------------------------------
-- fn_RoomTypeAvailableCount: sellable rooms of a given type still free for an ENTIRE date range.
-- OOO and OOS (Approved only) rooms are excluded from the sellable pool for any night they cover.
-- Demand = active, non-expired holds + PendingPayment/Confirmed/CheckedIn reservations that
-- overlap that night. Available for the range = MIN across nights of (capacity - demand), since
-- a room must be free for every night of the stay to be bookable for that stay.
-----------------------------------------------------------------------------------------------
CREATE OR ALTER FUNCTION dbo.fn_RoomTypeAvailableCount
(
    @HotelId UNIQUEIDENTIFIER,
    @RoomTypeId UNIQUEIDENTIFIER,
    @CheckInDate DATE,
    @CheckOutDate DATE
)
RETURNS INT
AS
BEGIN
    DECLARE @Available INT;
    DECLARE @TotalRooms INT;

    SELECT @TotalRooms = COUNT(*)
    FROM dbo.Rooms
    WHERE HotelId = @HotelId AND RoomTypeId = @RoomTypeId AND IsActive = 1;

    ;WITH Nights AS (
        SELECT DATEADD(DAY, value, @CheckInDate) AS StayDate
        FROM GENERATE_SERIES(0, DATEDIFF(DAY, @CheckInDate, @CheckOutDate) - 1)
    ),
    NightlyCapacity AS (
        SELECT n.StayDate,
               @TotalRooms - (
                   SELECT COUNT(*)
                   FROM dbo.RoomOutOfServicePeriods oos
                   JOIN dbo.Rooms r ON r.Id = oos.RoomId
                   WHERE r.HotelId = @HotelId AND r.RoomTypeId = @RoomTypeId
                     AND oos.Status = 1 /* Approved */
                     AND n.StayDate >= oos.StartDate AND n.StayDate < oos.EndDate
               ) AS Capacity
        FROM Nights n
    ),
    NightlyDemand AS (
        SELECT n.StayDate,
               (
                   ISNULL((SELECT SUM(h.RoomsRequested) FROM dbo.InventoryHolds h
                           WHERE h.HotelId = @HotelId AND h.RoomTypeId = @RoomTypeId AND h.Status = 0 /* Active */
                             AND h.ExpiresAtUtc > SYSUTCDATETIME()
                             AND n.StayDate >= h.CheckInDate AND n.StayDate < h.CheckOutDate), 0)
                   +
                   ISNULL((SELECT SUM(res.RoomsBooked) FROM dbo.Reservations res
                           WHERE res.HotelId = @HotelId AND res.RoomTypeId = @RoomTypeId
                             AND res.Status IN (0, 1, 2) /* PendingPayment, Confirmed, CheckedIn */
                             AND n.StayDate >= res.CheckInDate AND n.StayDate < res.CheckOutDate), 0)
               ) AS Demand
        FROM Nights n
    )
    SELECT @Available = MIN(c.Capacity - d.Demand)
    FROM NightlyCapacity c
    JOIN NightlyDemand d ON d.StayDate = c.StayDate;

    RETURN ISNULL(@Available, 0);
END;
GO

-----------------------------------------------------------------------------------------------
-- fn_ResolveNightlyRate: rate-selection priority = eligible corporate contract -> agent contract
-- -> explicitly selected public/package rate plan -> cheapest active public "base rate".
-----------------------------------------------------------------------------------------------
CREATE OR ALTER FUNCTION dbo.fn_ResolveNightlyRate
(
    @HotelId UNIQUEIDENTIFIER,
    @RoomTypeId UNIQUEIDENTIFIER,
    @StayDate DATE,
    @Occupancy INT,
    @RequestedRatePlanId UNIQUEIDENTIFIER,
    @CompanyId UNIQUEIDENTIFIER,
    @TravelAgentId UNIQUEIDENTIFIER
)
RETURNS @Result TABLE
(
    RatePlanId UNIQUEIDENTIFIER,
    RatePlanName NVARCHAR(150),
    MealPlan INT,
    Rate DECIMAL(18,2),
    RateSource VARCHAR(20)
)
AS
BEGIN
    DECLARE @Occ INT = CASE WHEN @Occupancy < 1 THEN 1 ELSE @Occupancy END;
    -- C# System.DayOfWeek convention (Sunday=0 .. Saturday=6), independent of session DATEFIRST.
    DECLARE @Dow INT = (DATEDIFF(DAY, '19000101', @StayDate) + 1) % 7;

    IF @CompanyId IS NOT NULL
        INSERT INTO @Result
        SELECT TOP 1 rp.Id, rp.Name, CAST(rp.MealPlan AS INT),
               ROUND(rpp.Rate * (1 - ISNULL(c.DiscountPercent, 0) / 100.0), 2), 'Corporate'
        FROM dbo.CorporateRateContracts c
        JOIN dbo.RatePlans rp ON rp.Id = c.RatePlanId AND rp.IsActive = 1
        JOIN dbo.RatePlanPrices rpp ON rpp.RatePlanId = rp.Id AND rpp.RoomTypeId = @RoomTypeId AND rpp.Occupancy = @Occ
        WHERE c.CompanyId = @CompanyId AND c.HotelId = @HotelId AND c.IsActive = 1
          AND @StayDate BETWEEN c.ContractStart AND c.ContractEnd
          AND @StayDate BETWEEN rpp.EffectiveFrom AND rpp.EffectiveTo
          AND (rpp.DayOfWeek IS NULL OR rpp.DayOfWeek = @Dow)
        ORDER BY CASE WHEN rpp.DayOfWeek IS NULL THEN 1 ELSE 0 END;

    IF NOT EXISTS (SELECT 1 FROM @Result) AND @TravelAgentId IS NOT NULL
        INSERT INTO @Result
        SELECT TOP 1 rp.Id, rp.Name, CAST(rp.MealPlan AS INT),
               ROUND(rpp.Rate * (1 - ISNULL(a.DiscountPercent, 0) / 100.0), 2), 'Agent'
        FROM dbo.AgentRateContracts a
        JOIN dbo.RatePlans rp ON rp.Id = a.RatePlanId AND rp.IsActive = 1
        JOIN dbo.RatePlanPrices rpp ON rpp.RatePlanId = rp.Id AND rpp.RoomTypeId = @RoomTypeId AND rpp.Occupancy = @Occ
        WHERE a.TravelAgentId = @TravelAgentId AND a.HotelId = @HotelId AND a.IsActive = 1
          AND @StayDate BETWEEN a.ContractStart AND a.ContractEnd
          AND @StayDate BETWEEN rpp.EffectiveFrom AND rpp.EffectiveTo
          AND (rpp.DayOfWeek IS NULL OR rpp.DayOfWeek = @Dow)
        ORDER BY CASE WHEN rpp.DayOfWeek IS NULL THEN 1 ELSE 0 END;

    IF NOT EXISTS (SELECT 1 FROM @Result) AND @RequestedRatePlanId IS NOT NULL
        INSERT INTO @Result
        SELECT TOP 1 rp.Id, rp.Name, CAST(rp.MealPlan AS INT), rpp.Rate, 'Selected'
        FROM dbo.RatePlans rp
        JOIN dbo.RatePlanPrices rpp ON rpp.RatePlanId = rp.Id AND rpp.RoomTypeId = @RoomTypeId AND rpp.Occupancy = @Occ
        WHERE rp.Id = @RequestedRatePlanId AND rp.IsActive = 1
          AND @StayDate BETWEEN rpp.EffectiveFrom AND rpp.EffectiveTo
          AND (rpp.DayOfWeek IS NULL OR rpp.DayOfWeek = @Dow)
        ORDER BY CASE WHEN rpp.DayOfWeek IS NULL THEN 1 ELSE 0 END;

    IF NOT EXISTS (SELECT 1 FROM @Result)
        INSERT INTO @Result
        SELECT TOP 1 rp.Id, rp.Name, CAST(rp.MealPlan AS INT), rpp.Rate, 'Base'
        FROM dbo.RatePlans rp
        JOIN dbo.RatePlanPrices rpp ON rpp.RatePlanId = rp.Id AND rpp.RoomTypeId = @RoomTypeId AND rpp.Occupancy = @Occ
        WHERE rp.HotelId = @HotelId AND rp.Scope = 0 /* Public */ AND rp.IsActive = 1
          AND @StayDate BETWEEN rpp.EffectiveFrom AND rpp.EffectiveTo
          AND (rpp.DayOfWeek IS NULL OR rpp.DayOfWeek = @Dow)
        ORDER BY rpp.Rate ASC, CASE WHEN rpp.DayOfWeek IS NULL THEN 1 ELSE 0 END;

    RETURN;
END;
GO

-----------------------------------------------------------------------------------------------
-- fn_ResolveRoomTariffGst: tariff-slab GST lookup for a room-charge amount on a given date.
-- Hotel-specific rules (HotelId set) take priority over the global default (HotelId NULL).
-- DISCLAIMER: illustrative demo slabs, not verified legal GST advice - see root README.
-----------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------
-- fn_ResolveGstRule: general-purpose version of fn_ResolveRoomTariffGst for any charge category
-- (room tariff, F&B, other services) - used by folio/POS charge posting.
-----------------------------------------------------------------------------------------------
CREATE OR ALTER FUNCTION dbo.fn_ResolveGstRule
(
    @HotelId UNIQUEIDENTIFIER,
    @ChargeCategory INT,
    @Amount DECIMAL(18,2),
    @StayDate DATE
)
RETURNS TABLE
AS
RETURN
(
    SELECT TOP 1 Id AS GstRuleId, CgstRate, SgstRate, IgstRate, HsnSac
    FROM dbo.GstRules
    WHERE ChargeCategory = @ChargeCategory
      AND IsActive = 1
      AND (HotelId = @HotelId OR HotelId IS NULL)
      AND @Amount >= ISNULL(MinAmount, 0)
      AND (MaxAmount IS NULL OR @Amount < MaxAmount)
      AND @StayDate >= EffectiveFrom
      AND (EffectiveTo IS NULL OR @StayDate <= EffectiveTo)
    ORDER BY CASE WHEN HotelId IS NOT NULL THEN 0 ELSE 1 END, EffectiveFrom DESC
);
GO

CREATE OR ALTER FUNCTION dbo.fn_ResolveRoomTariffGst
(
    @HotelId UNIQUEIDENTIFIER,
    @Amount DECIMAL(18,2),
    @StayDate DATE
)
RETURNS TABLE
AS
RETURN
(
    SELECT TOP 1 Id AS GstRuleId, CgstRate, SgstRate, IgstRate, HsnSac
    FROM dbo.GstRules
    WHERE ChargeCategory = 0 /* RoomTariff */
      AND IsActive = 1
      AND (HotelId = @HotelId OR HotelId IS NULL)
      AND @Amount >= ISNULL(MinAmount, 0)
      AND (MaxAmount IS NULL OR @Amount < MaxAmount)
      AND @StayDate >= EffectiveFrom
      AND (EffectiveTo IS NULL OR @StayDate <= EffectiveTo)
    ORDER BY CASE WHEN HotelId IS NOT NULL THEN 0 ELSE 1 END, EffectiveFrom DESC
);
GO
