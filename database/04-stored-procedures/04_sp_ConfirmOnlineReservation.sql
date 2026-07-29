/*
    sp_ConfirmOnlineReservation - converts a valid, still-active hold into a Confirmed reservation.
    This IS the payment-webhook target: it is idempotent on @IdempotencyKey (a retried/duplicate
    webhook call returns the same reservation instead of creating a second one), and is also the
    exact same "confirm" step reception bookings go through via sp_CreateReservation below.

    Also used directly by reception when a guest pays immediately at the desk (no separate payment
    webhook involved) - see sp_CreateReservation, which calls this proc right after creating a hold.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ConfirmOnlineReservation
    @HoldId UNIQUEIDENTIFIER,
    @IdempotencyKey NVARCHAR(100),
    @PaymentReference NVARCHAR(200) = NULL,
    @GuestId UNIQUEIDENTIFIER = NULL,
    @BillRoomChargeToCompany BIT = 0,
    @CreatedByUserId UNIQUEIDENTIFIER = NULL,
    @RecordPayment BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- Idempotent replay of the confirmation itself (e.g. a duplicated payment webhook delivery).
    IF EXISTS (SELECT 1 FROM dbo.Reservations WHERE IdempotencyKey = @IdempotencyKey)
    BEGIN
        SELECT * FROM dbo.Reservations WHERE IdempotencyKey = @IdempotencyKey;
        RETURN;
    END

    DECLARE @Hold TABLE (
        Id UNIQUEIDENTIFIER, HotelId UNIQUEIDENTIFIER, RoomTypeId UNIQUEIDENTIFIER, RatePlanId UNIQUEIDENTIFIER,
        CheckInDate DATE, CheckOutDate DATE, RoomsRequested INT, Adults INT, Children INT,
        Status INT, Source INT, ExpiresAtUtc DATETIME2, GuestId UNIQUEIDENTIFIER,
        CompanyId UNIQUEIDENTIFIER, TravelAgentId UNIQUEIDENTIFIER, ReservationId UNIQUEIDENTIFIER
    );

    INSERT INTO @Hold
    SELECT Id, HotelId, RoomTypeId, RatePlanId, CheckInDate, CheckOutDate, RoomsRequested, Adults, Children,
           Status, Source, ExpiresAtUtc, GuestId, CompanyId, TravelAgentId, ReservationId
    FROM dbo.InventoryHolds WHERE Id = @HoldId;

    IF NOT EXISTS (SELECT 1 FROM @Hold)
    BEGIN
        RAISERROR('Inventory hold not found.', 16, 1);
        RETURN;
    END

    -- Already confirmed under a different confirmation attempt - return the linked reservation rather than erroring.
    IF EXISTS (SELECT 1 FROM @Hold WHERE Status = 1 AND ReservationId IS NOT NULL)
    BEGIN
        SELECT r.* FROM dbo.Reservations r JOIN @Hold h ON h.ReservationId = r.Id;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM @Hold WHERE Status = 2)
    BEGIN
        RAISERROR('This hold has expired - please search availability again.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM @Hold WHERE Status = 3)
    BEGIN
        RAISERROR('This hold was released and can no longer be confirmed.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM @Hold WHERE Status = 0 AND ExpiresAtUtc <= SYSUTCDATETIME())
    BEGIN
        RAISERROR('This hold has expired - please search availability again.', 16, 1);
        RETURN;
    END

    DECLARE @FinalGuestId UNIQUEIDENTIFIER = (SELECT COALESCE(@GuestId, GuestId) FROM @Hold);
    IF @FinalGuestId IS NULL
    BEGIN
        RAISERROR('A guest must be specified either on the hold or at confirmation time.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;

    DECLARE @ReservationId UNIQUEIDENTIFIER = NEWID();
    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    DECLARE @ReservationNumber NVARCHAR(30) = CONCAT('STA-', FORMAT(SYSUTCDATETIME(), 'yyyyMMdd'), '-', RIGHT(CONCAT('000000', NEXT VALUE FOR dbo.ReservationNumberSeq), 6));

    DECLARE @HotelId UNIQUEIDENTIFIER, @RoomTypeId UNIQUEIDENTIFIER, @RatePlanId UNIQUEIDENTIFIER,
            @CheckInDate DATE, @CheckOutDate DATE, @RoomsRequested INT, @Adults INT, @Children INT,
            @Source INT, @CompanyId UNIQUEIDENTIFIER, @TravelAgentId UNIQUEIDENTIFIER, @BusinessDate DATE;

    SELECT @HotelId = HotelId, @RoomTypeId = RoomTypeId, @RatePlanId = RatePlanId,
           @CheckInDate = CheckInDate, @CheckOutDate = CheckOutDate, @RoomsRequested = RoomsRequested,
           @Adults = Adults, @Children = Children, @Source = Source, @CompanyId = CompanyId, @TravelAgentId = TravelAgentId
    FROM @Hold;

    SELECT @BusinessDate = BusinessDate FROM dbo.Hotels WHERE Id = @HotelId;

    INSERT INTO dbo.Reservations
        (Id, HotelId, ReservationNumber, GuestId, CompanyId, TravelAgentId, RoomTypeId, RatePlanId,
         CheckInDate, CheckOutDate, RoomsBooked, Adults, Children, Status, Source, InventoryHoldId,
         IdempotencyKey, BusinessDateCreated, CreatedByUserId, BillRoomChargeToCompany, CreatedAtUtc)
    VALUES
        (@ReservationId, @HotelId, @ReservationNumber, @FinalGuestId, @CompanyId, @TravelAgentId, @RoomTypeId, @RatePlanId,
         @CheckInDate, @CheckOutDate, @RoomsRequested, @Adults, @Children, 1 /* Confirmed */, @Source, @HoldId,
         @IdempotencyKey, @BusinessDate, @CreatedByUserId, @BillRoomChargeToCompany, @Now);

    -- Immutable per-night rate + GST-context snapshot - never recalculated from future rate/GST changes.
    DECLARE @Occupancy INT = CASE WHEN (@Adults + @Children) < 1 THEN 1 ELSE (@Adults + @Children) END;

    ;WITH Nights AS (
        SELECT DATEADD(DAY, value, @CheckInDate) AS StayDate
        FROM GENERATE_SERIES(0, DATEDIFF(DAY, @CheckInDate, @CheckOutDate) - 1)
    )
    INSERT INTO dbo.ReservationNightRates
        (Id, ReservationId, StayDate, RoomRate, MealPlan, InclusionsDescription, GstRuleId, CgstRate, SgstRate, IgstRate, CurrencyCode, CreatedAtUtc)
    SELECT
        NEWID(), @ReservationId, n.StayDate, rate.Rate, rate.MealPlan, CONCAT(rate.RatePlanName, ' (', rate.RateSource, ')'),
        gst.GstRuleId, gst.CgstRate, gst.SgstRate, gst.IgstRate, 'INR', @Now
    FROM Nights n
    CROSS APPLY dbo.fn_ResolveNightlyRate(@HotelId, @RoomTypeId, n.StayDate, @Occupancy, @RatePlanId, @CompanyId, @TravelAgentId) rate
    CROSS APPLY dbo.fn_ResolveRoomTariffGst(@HotelId, rate.Rate, n.StayDate) gst;

    -- Immutable cancellation-policy snapshot as of booking time.
    DECLARE @PolicyId UNIQUEIDENTIFIER = (SELECT CancellationPolicyId FROM dbo.RatePlans WHERE Id = @RatePlanId);

    IF @PolicyId IS NOT NULL
    BEGIN
        DECLARE @SnapshotId UNIQUEIDENTIFIER = NEWID();
        INSERT INTO dbo.ReservationPolicySnapshots (Id, ReservationId, SourceCancellationPolicyId, PolicyName, CreatedAtUtc)
        SELECT @SnapshotId, @ReservationId, Id, Name, @Now FROM dbo.CancellationPolicies WHERE Id = @PolicyId;

        INSERT INTO dbo.ReservationPolicySnapshotRules
            (Id, ReservationPolicySnapshotId, HoursBeforeCheckInMin, HoursBeforeCheckInMax, PenaltyType, PenaltyValue, AppliesToNoShow, SortOrder, Description, CreatedAtUtc)
        SELECT NEWID(), @SnapshotId, HoursBeforeCheckInMin, HoursBeforeCheckInMax, PenaltyType, PenaltyValue, AppliesToNoShow, SortOrder, Description, @Now
        FROM dbo.CancellationPolicyRules WHERE CancellationPolicyId = @PolicyId;
    END

    -- Online-gateway payment is recorded here (authoritative amount from the DB-computed night-rate
    -- snapshot, never trusted from the caller). Individual-guest hotel accommodation is always
    -- intra-state for GST purposes (place of supply = hotel location), so CGST+SGST is used here
    -- regardless of the guest's home state - see fn_ResolveRoomTariffGst / root README GST notes.
    IF @RecordPayment = 1
    BEGIN
        DECLARE @PaymentAmount DECIMAL(18,2) = (
            SELECT SUM(RoomRate + RoomRate * CgstRate / 100.0 + RoomRate * SgstRate / 100.0)
            FROM dbo.ReservationNightRates WHERE ReservationId = @ReservationId
        );

        INSERT INTO dbo.Payments (Id, ReservationId, FolioId, Amount, Method, Status, GatewayReference, IdempotencyKey, RecordedByUserId, CreatedAtUtc)
        VALUES (NEWID(), @ReservationId, NULL, @PaymentAmount, 4 /* OnlineGateway */, 1 /* Succeeded */, @PaymentReference, @IdempotencyKey, @CreatedByUserId, @Now);
    END

    UPDATE dbo.InventoryHolds
    SET Status = 1 /* Confirmed */, ReservationId = @ReservationId, UpdatedAtUtc = @Now
    WHERE Id = @HoldId;

    INSERT INTO dbo.AuditLogs (Id, UserId, HotelId, EntityType, EntityId, Action, DetailsJson, CreatedAtUtc)
    VALUES (NEWID(), @CreatedByUserId, @HotelId, 'Reservation', CAST(@ReservationId AS NVARCHAR(36)), 'Confirmed',
            (SELECT @PaymentReference AS paymentReference, @HoldId AS holdId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), @Now);

    COMMIT TRANSACTION;

    SELECT * FROM dbo.Reservations WHERE Id = @ReservationId;
END;
GO
