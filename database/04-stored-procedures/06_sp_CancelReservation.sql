/*
    sp_CancelReservation - the complete cancellation + penalty + refund-initiation workflow.
    @HoursBeforeCheckIn is computed by the application layer (using the hotel's IANA timezone via
    .NET TimeZoneInfo, since T-SQL's AT TIME ZONE only understands Windows zone names) as the
    number of hours between "now" in hotel-local time and midnight-local of the check-in date -
    that instant is this demo's documented stand-in for a formal check-in time.

    Idempotent: re-calling for an already-cancelled/no-show reservation returns the existing
    Cancellation row instead of erroring or double-charging a penalty.

    Future inventory release is implicit: fn_RoomTypeAvailableCount only counts reservations with
    Status IN (PendingPayment, Confirmed, CheckedIn), so flipping Status to Cancelled/NoShow here
    is itself the release - no separate "free up inventory" step is needed.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CancelReservation
    @ReservationId UNIQUEIDENTIFIER,
    @TriggerType INT, -- 0 = GuestCancellation, 1 = NoShow
    @HoursBeforeCheckIn INT = NULL, -- required when @TriggerType = 0
    @BusinessDate DATE,
    @Reason NVARCHAR(500) = NULL,
    @CancelledByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- Idempotent replay: already cancelled/no-show reservations just return their existing record.
    IF EXISTS (SELECT 1 FROM dbo.Cancellations WHERE ReservationId = @ReservationId)
    BEGIN
        SELECT c.*, r.Id AS RefundId, r.Status AS RefundStatus
        FROM dbo.Cancellations c
        LEFT JOIN dbo.Refunds r ON r.CancellationId = c.Id
        WHERE c.ReservationId = @ReservationId;
        RETURN;
    END

    DECLARE @Status INT, @HotelId UNIQUEIDENTIFIER;
    SELECT @Status = Status, @HotelId = HotelId FROM dbo.Reservations WHERE Id = @ReservationId;

    IF @Status IS NULL
    BEGIN
        RAISERROR('Reservation not found.', 16, 1);
        RETURN;
    END

    IF @Status NOT IN (1) -- Confirmed only; PendingPayment is handled by hold expiry, not this proc
    BEGIN
        RAISERROR('Only a Confirmed reservation can be cancelled through this workflow.', 16, 1);
        RETURN;
    END

    IF @TriggerType = 0 AND @HoursBeforeCheckIn IS NULL
    BEGIN
        RAISERROR('@HoursBeforeCheckIn is required for a guest-initiated cancellation.', 16, 1);
        RETURN;
    END

    DECLARE @SnapshotId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.ReservationPolicySnapshots WHERE ReservationId = @ReservationId);
    IF @SnapshotId IS NULL
    BEGIN
        RAISERROR('No cancellation policy snapshot exists for this reservation - cannot compute a penalty.', 16, 1);
        RETURN;
    END

    DECLARE @RuleId UNIQUEIDENTIFIER, @PenaltyType INT, @PenaltyValue DECIMAL(18,2);

    IF @TriggerType = 1 -- NoShow
        SELECT TOP 1 @RuleId = Id, @PenaltyType = PenaltyType, @PenaltyValue = PenaltyValue
        FROM dbo.ReservationPolicySnapshotRules
        WHERE ReservationPolicySnapshotId = @SnapshotId AND AppliesToNoShow = 1
        ORDER BY SortOrder DESC;
    ELSE
        SELECT TOP 1 @RuleId = Id, @PenaltyType = PenaltyType, @PenaltyValue = PenaltyValue
        FROM dbo.ReservationPolicySnapshotRules
        WHERE ReservationPolicySnapshotId = @SnapshotId
          AND @HoursBeforeCheckIn >= ISNULL(HoursBeforeCheckInMin, -2147483648)
          AND @HoursBeforeCheckIn < ISNULL(HoursBeforeCheckInMax, 2147483647)
        ORDER BY SortOrder;

    IF @RuleId IS NULL
    BEGIN
        RAISERROR('No cancellation policy rule matched this cancellation - check the policy configuration.', 16, 1);
        RETURN;
    END

    DECLARE @StayGross DECIMAL(18,2), @StayGrossGst DECIMAL(18,2), @FirstNightRate DECIMAL(18,2), @FirstNightCgst DECIMAL(18,2), @FirstNightSgst DECIMAL(18,2);

    SELECT @StayGross = SUM(RoomRate), @StayGrossGst = SUM(RoomRate * CgstRate / 100.0 + RoomRate * SgstRate / 100.0)
    FROM dbo.ReservationNightRates WHERE ReservationId = @ReservationId;

    SELECT TOP 1 @FirstNightRate = RoomRate, @FirstNightCgst = CgstRate, @FirstNightSgst = SgstRate
    FROM dbo.ReservationNightRates WHERE ReservationId = @ReservationId ORDER BY StayDate;

    DECLARE @PenaltyAmount DECIMAL(18,2) = CASE @PenaltyType
        WHEN 0 THEN 0
        WHEN 1 THEN @FirstNightRate
        WHEN 2 THEN ROUND(@StayGross * ISNULL(@PenaltyValue, 0) / 100.0, 2)
        WHEN 3 THEN @StayGross
        ELSE 0
    END;

    DECLARE @PenaltyGstAmount DECIMAL(18,2) = CASE @PenaltyType
        WHEN 1 THEN ROUND(@FirstNightRate * (@FirstNightCgst + @FirstNightSgst) / 100.0, 2)
        WHEN 3 THEN @StayGrossGst
        WHEN 2 THEN ROUND(@StayGrossGst * ISNULL(@PenaltyValue, 0) / 100.0, 2)
        ELSE 0
    END;

    DECLARE @RefundDue DECIMAL(18,2) = (@StayGross + @StayGrossGst) - (@PenaltyAmount + @PenaltyGstAmount);
    IF @RefundDue < 0 SET @RefundDue = 0;

    BEGIN TRANSACTION;

    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    DECLARE @CancellationId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO dbo.Cancellations
        (Id, ReservationId, TriggerType, CancelledAtUtc, HotelBusinessDateAtCancellation, CancelledByUserId,
         AppliedPolicyRuleId, HoursBeforeCheckIn, StayGrossAmount, PenaltyAmount, PenaltyGstAmount, RefundDueAmount, Reason, CreatedAtUtc)
    VALUES
        (@CancellationId, @ReservationId, @TriggerType, @Now, @BusinessDate, @CancelledByUserId,
         @RuleId, ISNULL(@HoursBeforeCheckIn, 0), @StayGross, @PenaltyAmount, @PenaltyGstAmount, @RefundDue, ISNULL(@Reason, ''), @Now);

    UPDATE dbo.Reservations
    SET Status = CASE WHEN @TriggerType = 1 THEN 5 /* NoShow */ ELSE 4 /* Cancelled */ END, UpdatedAtUtc = @Now
    WHERE Id = @ReservationId;

    DECLARE @RefundId UNIQUEIDENTIFIER = NULL;
    DECLARE @OriginalPaymentId UNIQUEIDENTIFIER = (
        SELECT TOP 1 Id FROM dbo.Payments
        WHERE ReservationId = @ReservationId AND Status = 1 /* Succeeded */
        ORDER BY CreatedAtUtc DESC
    );

    IF @OriginalPaymentId IS NOT NULL AND @RefundDue > 0
    BEGIN
        SET @RefundId = NEWID();
        INSERT INTO dbo.Refunds (Id, CancellationId, ReservationId, OriginalPaymentId, Amount, Status, RequestedAtUtc, CreatedAtUtc)
        VALUES (@RefundId, @CancellationId, @ReservationId, @OriginalPaymentId, @RefundDue, 0 /* RefundRequested */, @Now, @Now);
    END

    INSERT INTO dbo.AuditLogs (Id, UserId, HotelId, EntityType, EntityId, Action, DetailsJson, CreatedAtUtc)
    VALUES (NEWID(), @CancelledByUserId, @HotelId, 'Reservation', CAST(@ReservationId AS NVARCHAR(36)),
            CASE WHEN @TriggerType = 1 THEN 'NoShow' ELSE 'Cancelled' END,
            (SELECT @PenaltyAmount AS penaltyAmount, @RefundDue AS refundDue, @CancellationId AS cancellationId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), @Now);

    COMMIT TRANSACTION;

    SELECT c.*, @RefundId AS RefundId, r.Status AS RefundStatus
    FROM dbo.Cancellations c
    LEFT JOIN dbo.Refunds r ON r.Id = @RefundId
    WHERE c.Id = @CancellationId;
END;
GO
