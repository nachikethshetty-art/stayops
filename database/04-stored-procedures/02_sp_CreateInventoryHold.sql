/*
    sp_CreateInventoryHold - creates a 10-minute room-type inventory hold. Used directly by the
    online booking flow, and internally by sp_CreateReservation for reception bookings, so both
    paths run through the exact same overbooking-prevention check.

    Concurrency: sp_getapplock serializes concurrent hold attempts for the same
    (HotelId, RoomTypeId) so two simultaneous requests can never both observe "1 room available"
    and both succeed - the second one re-checks availability after acquiring the lock and fails
    cleanly if the first one consumed the last room.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateInventoryHold
    @HotelId UNIQUEIDENTIFIER,
    @RoomTypeId UNIQUEIDENTIFIER,
    @RatePlanId UNIQUEIDENTIFIER,
    @CheckInDate DATE,
    @CheckOutDate DATE,
    @RoomsRequested INT = 1,
    @Adults INT = 1,
    @Children INT = 0,
    @Source INT,
    @IdempotencyKey NVARCHAR(100),
    @GuestId UNIQUEIDENTIFIER = NULL,
    @CompanyId UNIQUEIDENTIFIER = NULL,
    @TravelAgentId UNIQUEIDENTIFIER = NULL,
    @CreatedByUserId UNIQUEIDENTIFIER = NULL,
    @HoldMinutes INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF EXISTS (SELECT 1 FROM dbo.InventoryHolds WHERE IdempotencyKey = @IdempotencyKey)
    BEGIN
        SELECT Id AS HoldId, HotelId, RoomTypeId, RatePlanId, CheckInDate, CheckOutDate, RoomsRequested,
               Status, Source, ExpiresAtUtc, GuestId, CompanyId, TravelAgentId, ReservationId
        FROM dbo.InventoryHolds
        WHERE IdempotencyKey = @IdempotencyKey;
        RETURN;
    END

    IF @CheckOutDate <= @CheckInDate
    BEGIN
        RAISERROR('CheckOutDate must be after CheckInDate.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;

    DECLARE @LockResource NVARCHAR(200) = CONCAT('StayOps:Hold:', @HotelId, ':', @RoomTypeId);
    DECLARE @LockResult INT;
    EXEC @LockResult = sp_getapplock @Resource = @LockResource, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000;

    IF @LockResult < 0
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Could not acquire the inventory lock for this room type - please retry.', 16, 1);
        RETURN;
    END

    DECLARE @Available INT = dbo.fn_RoomTypeAvailableCount(@HotelId, @RoomTypeId, @CheckInDate, @CheckOutDate);

    IF @Available < @RoomsRequested
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('No rooms of this type are available for the selected dates.', 16, 1);
        RETURN;
    END

    DECLARE @HoldId UNIQUEIDENTIFIER = NEWID();
    DECLARE @Now DATETIME2 = SYSUTCDATETIME();

    INSERT INTO dbo.InventoryHolds
        (Id, HotelId, RoomTypeId, RatePlanId, CheckInDate, CheckOutDate, RoomsRequested, Adults, Children,
         Status, Source, ExpiresAtUtc, IdempotencyKey, GuestId, CompanyId, TravelAgentId, CreatedByUserId, CreatedAtUtc)
    VALUES
        (@HoldId, @HotelId, @RoomTypeId, @RatePlanId, @CheckInDate, @CheckOutDate, @RoomsRequested, @Adults, @Children,
         0 /* Active */, @Source, DATEADD(MINUTE, @HoldMinutes, @Now), @IdempotencyKey, @GuestId, @CompanyId, @TravelAgentId, @CreatedByUserId, @Now);

    COMMIT TRANSACTION;

    SELECT Id AS HoldId, HotelId, RoomTypeId, RatePlanId, CheckInDate, CheckOutDate, RoomsRequested,
           Status, Source, ExpiresAtUtc, GuestId, CompanyId, TravelAgentId, ReservationId
    FROM dbo.InventoryHolds WHERE Id = @HoldId;
END;
GO
