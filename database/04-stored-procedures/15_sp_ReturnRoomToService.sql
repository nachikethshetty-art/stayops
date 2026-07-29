/*
    sp_ReturnRoomToService - closes out an OOO/OOS period early and, if the room's live status was
    OutOfOrder/OutOfService because of it, flips it back to Available.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ReturnRoomToService
    @PeriodId UNIQUEIDENTIFIER,
    @ReturnedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @RoomId UNIQUEIDENTIFIER, @Status INT;
    SELECT @RoomId = RoomId, @Status = Status FROM dbo.RoomOutOfServicePeriods WHERE Id = @PeriodId;

    IF @RoomId IS NULL
    BEGIN
        RAISERROR('OOO/OOS period not found.', 16, 1);
        RETURN;
    END

    IF @Status <> 1 /* Approved */
    BEGIN
        RAISERROR('Only an approved, currently-in-effect period can be returned to service.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;

    DECLARE @Now DATETIME2 = SYSUTCDATETIME();

    UPDATE dbo.RoomOutOfServicePeriods
    SET Status = 2 /* ReturnedToService */, ReturnedToServiceAtUtc = @Now, ReturnedByUserId = @ReturnedByUserId, UpdatedAtUtc = @Now
    WHERE Id = @PeriodId;

    DECLARE @CurrentRoomStatus INT = (SELECT Status FROM dbo.Rooms WHERE Id = @RoomId);
    IF @CurrentRoomStatus IN (4, 5) /* OutOfService, OutOfOrder */
    BEGIN
        INSERT INTO dbo.RoomStatusHistories (Id, RoomId, FromStatus, ToStatus, Reason, ChangedByUserId, ChangedAtUtc, CreatedAtUtc)
        VALUES (NEWID(), @RoomId, @CurrentRoomStatus, 0 /* Available */, 'Returned to service', @ReturnedByUserId, @Now, @Now);

        UPDATE dbo.Rooms SET Status = 0 /* Available */, UpdatedAtUtc = @Now WHERE Id = @RoomId;
    END

    COMMIT TRANSACTION;

    SELECT * FROM dbo.RoomOutOfServicePeriods WHERE Id = @PeriodId;
END;
GO
