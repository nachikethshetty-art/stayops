/*
    sp_CreateReservation - the reception/walk-in booking entry point. It deliberately does NOT
    reimplement availability checking or reservation creation: it calls sp_CreateInventoryHold and
    then sp_ConfirmOnlineReservation, the exact same procedures the online booking payment webhook
    uses, so reception and online booking share one inventory mechanism end to end as required by
    the README. Reception simply skips the asynchronous payment wait, since payment/billing is
    settled immediately at the desk (or billed to a folio) rather than through the mock gateway.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateReservation
    @HotelId UNIQUEIDENTIFIER,
    @RoomTypeId UNIQUEIDENTIFIER,
    @RatePlanId UNIQUEIDENTIFIER,
    @CheckInDate DATE,
    @CheckOutDate DATE,
    @GuestId UNIQUEIDENTIFIER,
    @RoomsRequested INT = 1,
    @Adults INT = 1,
    @Children INT = 0,
    @IdempotencyKey NVARCHAR(100),
    @CompanyId UNIQUEIDENTIFIER = NULL,
    @TravelAgentId UNIQUEIDENTIFIER = NULL,
    @BillRoomChargeToCompany BIT = 0,
    @CreatedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @HoldResult TABLE (
        HoldId UNIQUEIDENTIFIER, HotelId UNIQUEIDENTIFIER, RoomTypeId UNIQUEIDENTIFIER, RatePlanId UNIQUEIDENTIFIER,
        CheckInDate DATE, CheckOutDate DATE, RoomsRequested INT, Status INT, Source INT, ExpiresAtUtc DATETIME2,
        GuestId UNIQUEIDENTIFIER, CompanyId UNIQUEIDENTIFIER, TravelAgentId UNIQUEIDENTIFIER, ReservationId UNIQUEIDENTIFIER
    );

    INSERT INTO @HoldResult
    EXEC dbo.sp_CreateInventoryHold
        @HotelId = @HotelId, @RoomTypeId = @RoomTypeId, @RatePlanId = @RatePlanId,
        @CheckInDate = @CheckInDate, @CheckOutDate = @CheckOutDate, @RoomsRequested = @RoomsRequested,
        @Adults = @Adults, @Children = @Children, @Source = 1 /* Reception */, @IdempotencyKey = @IdempotencyKey,
        @GuestId = @GuestId, @CompanyId = @CompanyId, @TravelAgentId = @TravelAgentId, @CreatedByUserId = @CreatedByUserId;

    DECLARE @HoldId UNIQUEIDENTIFIER = (SELECT TOP 1 HoldId FROM @HoldResult);

    IF @HoldId IS NULL
    BEGIN
        RAISERROR('Failed to create inventory hold for reservation.', 16, 1);
        RETURN;
    END

    -- If the hold already had a reservation confirmed against it (idempotent replay), this call
    -- itself is idempotent and simply returns that same reservation.
    -- Reception bookings don't auto-record a gateway payment - settlement happens via folio/payment at check-in or checkout (see sp_RecordFolioPayment).
    EXEC dbo.sp_ConfirmOnlineReservation
        @HoldId = @HoldId, @IdempotencyKey = @IdempotencyKey, @PaymentReference = 'RECEPTION-DIRECT',
        @GuestId = @GuestId, @BillRoomChargeToCompany = @BillRoomChargeToCompany, @CreatedByUserId = @CreatedByUserId,
        @RecordPayment = 0;
END;
GO
