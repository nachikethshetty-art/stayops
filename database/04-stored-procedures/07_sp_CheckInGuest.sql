/*
    sp_CheckInGuest - assigns a physical room (room-type inventory becomes a real room only now)
    and opens the folios required for the stay. Room charges are NOT posted here - Night Audit
    posts room rent once per night for every checked-in stay (see sp_RunNightAudit).

    Idempotent: re-calling for an already-CheckedIn reservation just returns its existing folios.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CheckInGuest
    @ReservationId UNIQUEIDENTIFIER,
    @RoomId UNIQUEIDENTIFIER,
    @CheckedInByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @HotelId UNIQUEIDENTIFIER, @RoomTypeId UNIQUEIDENTIFIER, @Status INT, @CompanyId UNIQUEIDENTIFIER, @BillToCompany BIT, @ReservationCheckIn DATE;
    SELECT @HotelId = HotelId, @RoomTypeId = RoomTypeId, @Status = Status, @CompanyId = CompanyId,
           @BillToCompany = BillRoomChargeToCompany, @ReservationCheckIn = CheckInDate
    FROM dbo.Reservations WHERE Id = @ReservationId;

    IF @HotelId IS NULL
    BEGIN
        RAISERROR('Reservation not found.', 16, 1);
        RETURN;
    END

    -- Idempotent replay.
    IF @Status = 2 /* CheckedIn */
    BEGIN
        SELECT Id AS FolioId, Type AS FolioType, Status AS FolioStatus, Balance
        FROM dbo.Folios WHERE ReservationId = @ReservationId;
        RETURN;
    END

    IF @Status <> 1 /* Confirmed */
    BEGIN
        RAISERROR('Only a Confirmed reservation can be checked in.', 16, 1);
        RETURN;
    END

    DECLARE @HotelBusinessDate DATE = (SELECT BusinessDate FROM dbo.Hotels WHERE Id = @HotelId);
    IF @ReservationCheckIn > @HotelBusinessDate
    BEGIN
        RAISERROR('Cannot check in before the reservation''s scheduled arrival date (hotel business date has not reached it yet).', 16, 1);
        RETURN;
    END

    DECLARE @RoomHotelId UNIQUEIDENTIFIER, @RoomTypeIdActual UNIQUEIDENTIFIER, @RoomStatus INT;
    SELECT @RoomHotelId = HotelId, @RoomTypeIdActual = RoomTypeId, @RoomStatus = Status
    FROM dbo.Rooms WHERE Id = @RoomId;

    IF @RoomHotelId IS NULL OR @RoomHotelId <> @HotelId
    BEGIN
        RAISERROR('Room does not belong to this reservation''s hotel.', 16, 1);
        RETURN;
    END

    IF @RoomTypeIdActual <> @RoomTypeId
    BEGIN
        RAISERROR('Room is not of the room type booked on this reservation.', 16, 1);
        RETURN;
    END

    IF @RoomStatus NOT IN (0, 1) /* Available or Reserved */
    BEGIN
        RAISERROR('Room is not available to check in a guest right now.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;

    DECLARE @Now DATETIME2 = SYSUTCDATETIME();

    INSERT INTO dbo.ReservationRoomAssignments (Id, ReservationId, RoomId, AssignedAtUtc, CreatedAtUtc)
    VALUES (NEWID(), @ReservationId, @RoomId, @Now, @Now);

    INSERT INTO dbo.RoomStatusHistories (Id, RoomId, FromStatus, ToStatus, Reason, ChangedByUserId, ChangedAtUtc, CreatedAtUtc)
    VALUES (NEWID(), @RoomId, @RoomStatus, 2 /* Occupied */, 'Guest checked in', @CheckedInByUserId, @Now, @Now);

    UPDATE dbo.Rooms SET Status = 2 /* Occupied */, UpdatedAtUtc = @Now WHERE Id = @RoomId;

    UPDATE dbo.Reservations SET Status = 2 /* CheckedIn */, UpdatedAtUtc = @Now WHERE Id = @ReservationId;

    DECLARE @GuestFolioId UNIQUEIDENTIFIER = NEWID();
    INSERT INTO dbo.Folios (Id, ReservationId, Type, OwnerCompanyId, Status, Balance, OpenedAtUtc, CreatedAtUtc)
    VALUES (@GuestFolioId, @ReservationId, 0 /* Guest */, NULL, 0 /* Open */, 0, @Now, @Now);

    DECLARE @CompanyFolioId UNIQUEIDENTIFIER = NULL;
    IF @BillToCompany = 1 AND @CompanyId IS NOT NULL
    BEGIN
        SET @CompanyFolioId = NEWID();
        INSERT INTO dbo.Folios (Id, ReservationId, Type, OwnerCompanyId, Status, Balance, OpenedAtUtc, CreatedAtUtc)
        VALUES (@CompanyFolioId, @ReservationId, 1 /* Company */, @CompanyId, 0 /* Open */, 0, @Now, @Now);
    END

    -- Reconcile any pre-existing online-gateway payment onto the new guest folio as a credit.
    DECLARE @PaymentId UNIQUEIDENTIFIER, @PaymentAmount DECIMAL(18,2);
    SELECT TOP 1 @PaymentId = Id, @PaymentAmount = Amount
    FROM dbo.Payments
    WHERE ReservationId = @ReservationId AND Status = 1 /* Succeeded */ AND FolioId IS NULL
    ORDER BY CreatedAtUtc;

    IF @PaymentId IS NOT NULL
    BEGIN
        UPDATE dbo.Payments SET FolioId = @GuestFolioId WHERE Id = @PaymentId;

        DECLARE @PaymentTxnId UNIQUEIDENTIFIER = NEWID();
        INSERT INTO dbo.FolioTransactions
            (Id, FolioId, Type, Description, Amount, GstAmount, TotalAmount, BusinessDate, PostedByUserId, SourceReference, UniquePostingKey, CreatedAtUtc)
        VALUES
            (@PaymentTxnId, @GuestFolioId, 3 /* Payment */, 'Online payment received at booking', @PaymentAmount, 0, -@PaymentAmount,
             @HotelBusinessDate, @CheckedInByUserId, CAST(@PaymentId AS NVARCHAR(36)), CONCAT('CHECKIN-PAYMENT-LINK:', @PaymentId), @Now);

        UPDATE dbo.Payments SET FolioTransactionId = @PaymentTxnId WHERE Id = @PaymentId;
        UPDATE dbo.Folios SET Balance = Balance - @PaymentAmount WHERE Id = @GuestFolioId;
    END

    INSERT INTO dbo.AuditLogs (Id, UserId, HotelId, EntityType, EntityId, Action, DetailsJson, CreatedAtUtc)
    VALUES (NEWID(), @CheckedInByUserId, @HotelId, 'Reservation', CAST(@ReservationId AS NVARCHAR(36)), 'CheckedIn',
            (SELECT @RoomId AS roomId, @GuestFolioId AS guestFolioId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), @Now);

    COMMIT TRANSACTION;

    SELECT Id AS FolioId, Type AS FolioType, Status AS FolioStatus, Balance
    FROM dbo.Folios WHERE ReservationId = @ReservationId;
END;
GO
