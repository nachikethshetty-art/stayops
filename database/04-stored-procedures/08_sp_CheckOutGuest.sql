/*
    sp_CheckOutGuest - validates the guest folio is settled (balance <= 0), closes all folios for
    the stay, unassigns the physical room and marks it Dirty, and creates the post-checkout
    housekeeping task. Company/direct-bill folios may still show a balance owed by the company at
    checkout time - that is expected (they get invoiced/collected later via sp_GenerateGstInvoice)
    and does not block checkout; only the guest's own folio must be settled unless @ForceCheckout=1.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CheckOutGuest
    @ReservationId UNIQUEIDENTIFIER,
    @CheckedOutByUserId UNIQUEIDENTIFIER = NULL,
    @ForceCheckout BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @HotelId UNIQUEIDENTIFIER, @Status INT;
    SELECT @HotelId = HotelId, @Status = Status FROM dbo.Reservations WHERE Id = @ReservationId;

    IF @HotelId IS NULL
    BEGIN
        RAISERROR('Reservation not found.', 16, 1);
        RETURN;
    END

    IF @Status = 3 /* CheckedOut */
    BEGIN
        SELECT Id AS FolioId, Type AS FolioType, Status AS FolioStatus, Balance FROM dbo.Folios WHERE ReservationId = @ReservationId;
        RETURN;
    END

    IF @Status <> 2 /* CheckedIn */
    BEGIN
        RAISERROR('Only a CheckedIn reservation can be checked out.', 16, 1);
        RETURN;
    END

    DECLARE @GuestFolioBalance DECIMAL(18,2) = (
        SELECT ISNULL(SUM(Balance), 0) FROM dbo.Folios WHERE ReservationId = @ReservationId AND Type = 0 /* Guest */
    );

    IF @GuestFolioBalance > 0 AND @ForceCheckout = 0
    BEGIN
        DECLARE @BalanceMsg NVARCHAR(50) = CAST(@GuestFolioBalance AS NVARCHAR(50));
        RAISERROR('Guest folio has an outstanding balance of %s - settle it or use a forced checkout override.', 16, 1, @BalanceMsg);
        RETURN;
    END

    BEGIN TRANSACTION;

    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    DECLARE @HotelBusinessDate DATE = (SELECT BusinessDate FROM dbo.Hotels WHERE Id = @HotelId);

    UPDATE dbo.Folios SET Status = 1 /* Closed */, ClosedAtUtc = @Now, UpdatedAtUtc = @Now
    WHERE ReservationId = @ReservationId AND Status = 0 /* Open */;

    DECLARE @RoomIds TABLE (RoomId UNIQUEIDENTIFIER);
    UPDATE dbo.ReservationRoomAssignments
    SET UnassignedAtUtc = @Now
    OUTPUT inserted.RoomId INTO @RoomIds
    WHERE ReservationId = @ReservationId AND UnassignedAtUtc IS NULL;

    UPDATE r
    SET r.Status = 3 /* Dirty */, r.UpdatedAtUtc = @Now
    FROM dbo.Rooms r
    JOIN @RoomIds x ON x.RoomId = r.Id;

    INSERT INTO dbo.RoomStatusHistories (Id, RoomId, FromStatus, ToStatus, Reason, ChangedByUserId, ChangedAtUtc, CreatedAtUtc)
    SELECT NEWID(), RoomId, 2 /* Occupied */, 3 /* Dirty */, 'Guest checked out', @CheckedOutByUserId, @Now, @Now
    FROM @RoomIds;

    INSERT INTO dbo.HousekeepingTasks (Id, HotelId, RoomId, TaskType, Status, Notes, CreatedAtUtc)
    SELECT NEWID(), @HotelId, RoomId, 0 /* CleanAfterCheckout */, 0 /* Pending */, 'Post-checkout clean', @Now
    FROM @RoomIds;

    UPDATE dbo.Reservations SET Status = 3 /* CheckedOut */, UpdatedAtUtc = @Now WHERE Id = @ReservationId;

    INSERT INTO dbo.AuditLogs (Id, UserId, HotelId, EntityType, EntityId, Action, DetailsJson, CreatedAtUtc)
    VALUES (NEWID(), @CheckedOutByUserId, @HotelId, 'Reservation', CAST(@ReservationId AS NVARCHAR(36)), 'CheckedOut',
            (SELECT @GuestFolioBalance AS guestFolioBalanceAtCheckout, @ForceCheckout AS forced FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), @Now);

    COMMIT TRANSACTION;

    SELECT Id AS FolioId, Type AS FolioType, Status AS FolioStatus, Balance FROM dbo.Folios WHERE ReservationId = @ReservationId;
END;
GO
