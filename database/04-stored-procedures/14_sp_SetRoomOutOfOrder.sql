/*
    sp_SetRoomOutOfOrder - records an approved OOO/OOS date range for a room. Availability
    (fn_RoomTypeAvailableCount) reads RoomOutOfServicePeriods directly and is therefore always
    date-range correct regardless of "today"; this proc additionally flips the room's live
    Room.Status when the period covers the hotel's CURRENT business date, so the reception/
    housekeeping board reflects it immediately. Periods that start in the future are swept and
    applied to Room.Status by sp_RunNightAudit as the business date advances into them.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_SetRoomOutOfOrder
    @RoomId UNIQUEIDENTIFIER,
    @Type INT, -- RoomOutOfServiceType: 0 = OutOfOrder, 1 = OutOfService
    @StartDate DATE,
    @EndDate DATE,
    @Reason NVARCHAR(500),
    @RequestedByUserId UNIQUEIDENTIFIER = NULL,
    @ApprovedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @EndDate <= @StartDate
    BEGIN
        RAISERROR('EndDate must be after StartDate.', 16, 1);
        RETURN;
    END

    DECLARE @HotelId UNIQUEIDENTIFIER, @HotelBusinessDate DATE, @CurrentRoomStatus INT;
    SELECT @HotelId = r.HotelId, @CurrentRoomStatus = r.Status FROM dbo.Rooms r WHERE r.Id = @RoomId;
    IF @HotelId IS NULL
    BEGIN
        RAISERROR('Room not found.', 16, 1);
        RETURN;
    END
    SELECT @HotelBusinessDate = BusinessDate FROM dbo.Hotels WHERE Id = @HotelId;

    IF @CurrentRoomStatus = 2 /* Occupied */
    BEGIN
        RAISERROR('Cannot take an occupied room out of service - check the guest out or move them first.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;

    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    DECLARE @PeriodId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO dbo.RoomOutOfServicePeriods
        (Id, RoomId, Type, StartDate, EndDate, Reason, Status, RequestedByUserId, ApprovedByUserId, ApprovedAtUtc, CreatedAtUtc)
    VALUES
        (@PeriodId, @RoomId, @Type, @StartDate, @EndDate, @Reason, 1 /* Approved */, @RequestedByUserId, @ApprovedByUserId, @Now, @Now);

    IF @HotelBusinessDate >= @StartDate AND @HotelBusinessDate < @EndDate
    BEGIN
        DECLARE @NewRoomStatus INT = CASE WHEN @Type = 0 THEN 5 /* OutOfOrder */ ELSE 4 /* OutOfService */ END;

        INSERT INTO dbo.RoomStatusHistories (Id, RoomId, FromStatus, ToStatus, Reason, ChangedByUserId, ChangedAtUtc, CreatedAtUtc)
        VALUES (NEWID(), @RoomId, @CurrentRoomStatus, @NewRoomStatus, @Reason, @ApprovedByUserId, @Now, @Now);

        UPDATE dbo.Rooms SET Status = @NewRoomStatus, UpdatedAtUtc = @Now WHERE Id = @RoomId;
    END

    INSERT INTO dbo.AuditLogs (Id, UserId, HotelId, EntityType, EntityId, Action, DetailsJson, CreatedAtUtc)
    VALUES (NEWID(), @ApprovedByUserId, @HotelId, 'Room', CAST(@RoomId AS NVARCHAR(36)), 'SetOutOfOrder',
            (SELECT @Type AS type, @StartDate AS startDate, @EndDate AS endDate FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), @Now);

    COMMIT TRANSACTION;

    SELECT * FROM dbo.RoomOutOfServicePeriods WHERE Id = @PeriodId;
END;
GO
