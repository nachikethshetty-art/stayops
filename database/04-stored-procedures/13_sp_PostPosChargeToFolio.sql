/*
    sp_PostPosChargeToFolio - POS outlet posts a charge to the guest folio of whoever is currently
    checked into a room. Idempotency key = OutletCode + ':' + PosReferenceNumber: a duplicate
    request (same outlet, same POS transaction reference) returns the original result instead of
    posting twice. Delegates the actual ledger mechanics to sp_PostFolioCharge (the same primitive
    used by manual folio charges and Night Audit room-charge posting) so there is one charge-
    posting implementation, not three.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_PostPosChargeToFolio
    @HotelId UNIQUEIDENTIFIER,
    @OutletCode NVARCHAR(20),
    @PosReferenceNumber NVARCHAR(100),
    @RoomNumber NVARCHAR(20),
    @Amount DECIMAL(18,2),
    @Description NVARCHAR(500),
    @ChargeCategory INT -- GstChargeCategory: 0 RoomTariff (unusual for POS but allowed), 1 FoodAndBeverage, 2 OtherServices
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdempotencyKey NVARCHAR(150) = CONCAT(@OutletCode, ':', @PosReferenceNumber);

    IF EXISTS (SELECT 1 FROM dbo.PosCharges WHERE IdempotencyKey = @IdempotencyKey)
    BEGIN
        SELECT pc.*, 1 AS WasDuplicate, f.Balance AS FolioBalance
        FROM dbo.PosCharges pc JOIN dbo.Folios f ON f.Id = pc.FolioId
        WHERE pc.IdempotencyKey = @IdempotencyKey;
        RETURN;
    END

    DECLARE @OutletId UNIQUEIDENTIFIER, @OutletHotelId UNIQUEIDENTIFIER, @CreditLimit DECIMAL(18,2);
    SELECT @OutletId = Id, @OutletHotelId = HotelId, @CreditLimit = DefaultCreditLimit
    FROM dbo.PosOutlets WHERE Code = @OutletCode AND IsActive = 1;

    IF @OutletId IS NULL
    BEGIN
        RAISERROR('POS outlet not found or inactive.', 16, 1);
        RETURN;
    END

    IF @OutletHotelId <> @HotelId
    BEGIN
        RAISERROR('POS outlet does not belong to this hotel.', 16, 1);
        RETURN;
    END

    DECLARE @RoomId UNIQUEIDENTIFIER;
    SELECT @RoomId = Id FROM dbo.Rooms WHERE HotelId = @HotelId AND RoomNumber = @RoomNumber;
    IF @RoomId IS NULL
    BEGIN
        RAISERROR('Room not found at this hotel.', 16, 1);
        RETURN;
    END

    DECLARE @ReservationId UNIQUEIDENTIFIER;
    DECLARE @MatchCount INT;
    SELECT @MatchCount = COUNT(*)
    FROM dbo.ReservationRoomAssignments a
    JOIN dbo.Reservations r ON r.Id = a.ReservationId
    WHERE a.RoomId = @RoomId AND a.UnassignedAtUtc IS NULL AND r.Status = 2 /* CheckedIn */;

    IF @MatchCount = 0
    BEGIN
        RAISERROR('No checked-in stay found for this room.', 16, 1);
        RETURN;
    END
    IF @MatchCount > 1
    BEGIN
        RAISERROR('Data integrity error: more than one checked-in stay found for this room.', 16, 1);
        RETURN;
    END

    SELECT @ReservationId = a.ReservationId
    FROM dbo.ReservationRoomAssignments a
    JOIN dbo.Reservations r ON r.Id = a.ReservationId
    WHERE a.RoomId = @RoomId AND a.UnassignedAtUtc IS NULL AND r.Status = 2;

    DECLARE @FolioId UNIQUEIDENTIFIER, @CurrentBalance DECIMAL(18,2);
    SELECT @FolioId = Id, @CurrentBalance = Balance
    FROM dbo.Folios WHERE ReservationId = @ReservationId AND Type = 0 /* Guest */ AND Status = 0 /* Open */;

    IF @FolioId IS NULL
    BEGIN
        RAISERROR('No open guest folio found for this stay.', 16, 1);
        RETURN;
    END

    DECLARE @BusinessDate DATE = (SELECT BusinessDate FROM dbo.Hotels WHERE Id = @HotelId);
    DECLARE @CgstRate DECIMAL(18,2), @SgstRate DECIMAL(18,2);
    SELECT @CgstRate = CgstRate, @SgstRate = SgstRate FROM dbo.fn_ResolveGstRule(@HotelId, @ChargeCategory, @Amount, @BusinessDate);

    DECLARE @ProjectedTotal DECIMAL(18,2) = @Amount * (1 + ISNULL(@CgstRate, 0) / 100.0 + ISNULL(@SgstRate, 0) / 100.0);
    IF @CurrentBalance + @ProjectedTotal > @CreditLimit
    BEGIN
        RAISERROR('Posting this charge would exceed the guest folio credit limit for this outlet.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;

    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    DECLARE @PosSourceReference NVARCHAR(200) = CONCAT('POS:', @OutletCode, ':', @PosReferenceNumber);

    DECLARE @TxnResult TABLE (
        Id UNIQUEIDENTIFIER, FolioId UNIQUEIDENTIFIER, Type INT, Description NVARCHAR(500),
        Amount DECIMAL(18,2), GstAmount DECIMAL(18,2), TotalAmount DECIMAL(18,2),
        ReversalOfTransactionId UNIQUEIDENTIFIER, BusinessDate DATE, PostedByUserId UNIQUEIDENTIFIER,
        SourceReference NVARCHAR(200), UniquePostingKey NVARCHAR(200), CreatedAtUtc DATETIME2, UpdatedAtUtc DATETIME2
    );

    INSERT INTO @TxnResult
    EXEC dbo.sp_PostFolioCharge
        @FolioId = @FolioId, @ChargeType = 1 /* Incidental */, @ChargeCategory = @ChargeCategory,
        @Description = @Description, @TaxableAmount = @Amount, @SourceReference = @PosSourceReference,
        @UniquePostingKey = @IdempotencyKey, @BusinessDateOverride = @BusinessDate;

    DECLARE @FolioTransactionId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM @TxnResult);

    DECLARE @PosChargeId UNIQUEIDENTIFIER = NEWID();
    INSERT INTO dbo.PosCharges (Id, PosOutletId, PosReferenceNumber, IdempotencyKey, RoomId, ReservationId, FolioId, FolioTransactionId, Amount, Description, CreatedAtUtc)
    VALUES (@PosChargeId, @OutletId, @PosReferenceNumber, @IdempotencyKey, @RoomId, @ReservationId, @FolioId, @FolioTransactionId, @Amount, @Description, @Now);

    INSERT INTO dbo.AuditLogs (Id, HotelId, EntityType, EntityId, Action, DetailsJson, CreatedAtUtc)
    VALUES (NEWID(), @HotelId, 'PosCharge', CAST(@PosChargeId AS NVARCHAR(36)), 'Posted',
            (SELECT @OutletCode AS outletCode, @PosReferenceNumber AS posReference, @Amount AS amount FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), @Now);

    COMMIT TRANSACTION;

    SELECT pc.*, 0 AS WasDuplicate, f.Balance AS FolioBalance
    FROM dbo.PosCharges pc JOIN dbo.Folios f ON f.Id = pc.FolioId
    WHERE pc.Id = @PosChargeId;
END;
GO
