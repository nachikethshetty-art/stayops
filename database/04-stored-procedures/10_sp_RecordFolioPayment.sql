/*
    sp_RecordFolioPayment - records a payment (cash/card/UPI/bank transfer collected at the desk)
    against an open folio. Idempotent on @IdempotencyKey. Never overwrites another payment; each
    call inserts a brand-new Payment + FolioTransaction pair.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_RecordFolioPayment
    @FolioId UNIQUEIDENTIFIER,
    @Amount DECIMAL(18,2),
    @Method INT,
    @GatewayReference NVARCHAR(200) = NULL,
    @IdempotencyKey NVARCHAR(100) = NULL,
    @RecordedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @IdempotencyKey IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.Payments WHERE IdempotencyKey = @IdempotencyKey)
    BEGIN
        SELECT p.*, f.Balance AS FolioBalance FROM dbo.Payments p JOIN dbo.Folios f ON f.Id = p.FolioId WHERE p.IdempotencyKey = @IdempotencyKey;
        RETURN;
    END

    DECLARE @ReservationId UNIQUEIDENTIFIER, @Status INT, @HotelId UNIQUEIDENTIFIER;
    SELECT @ReservationId = f.ReservationId, @Status = f.Status, @HotelId = r.HotelId
    FROM dbo.Folios f JOIN dbo.Reservations r ON r.Id = f.ReservationId
    WHERE f.Id = @FolioId;

    IF @ReservationId IS NULL
    BEGIN
        RAISERROR('Folio not found.', 16, 1);
        RETURN;
    END

    IF @Status <> 0 /* Open */
    BEGIN
        RAISERROR('Cannot record a payment against a closed folio.', 16, 1);
        RETURN;
    END

    IF @Amount <= 0
    BEGIN
        RAISERROR('Payment amount must be positive.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;

    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    DECLARE @BusinessDate DATE = (SELECT BusinessDate FROM dbo.Hotels WHERE Id = @HotelId);
    DECLARE @PaymentId UNIQUEIDENTIFIER = NEWID();
    DECLARE @TxnId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO dbo.Payments (Id, ReservationId, FolioId, Amount, Method, Status, GatewayReference, IdempotencyKey, RecordedByUserId, FolioTransactionId, CreatedAtUtc)
    VALUES (@PaymentId, @ReservationId, @FolioId, @Amount, @Method, 1 /* Succeeded */, @GatewayReference, @IdempotencyKey, @RecordedByUserId, @TxnId, @Now);

    INSERT INTO dbo.FolioTransactions (Id, FolioId, Type, Description, Amount, GstAmount, TotalAmount, BusinessDate, PostedByUserId, SourceReference, CreatedAtUtc)
    VALUES (@TxnId, @FolioId, 3 /* Payment */, 'Payment received', @Amount, 0, -@Amount, @BusinessDate, @RecordedByUserId, CAST(@PaymentId AS NVARCHAR(36)), @Now);

    UPDATE dbo.Folios SET Balance = Balance - @Amount, UpdatedAtUtc = @Now WHERE Id = @FolioId;

    COMMIT TRANSACTION;

    SELECT p.*, f.Balance AS FolioBalance FROM dbo.Payments p JOIN dbo.Folios f ON f.Id = p.FolioId WHERE p.Id = @PaymentId;
END;
GO
