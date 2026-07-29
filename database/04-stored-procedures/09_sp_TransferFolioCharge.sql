/*
    sp_TransferFolioCharge - moves a single charge from one folio to another within the same
    stay (e.g. an incidental mistakenly posted to the guest folio, moved to the company folio).
    Implemented as reverse-then-repost, never as an edit: the original transaction is left
    untouched and a Reversal transaction is posted against it, giving a complete audit trail
    (FolioTransfers row links the reversal and the new charge together).
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_TransferFolioCharge
    @SourceTransactionId UNIQUEIDENTIFIER,
    @DestinationFolioId UNIQUEIDENTIFIER,
    @Reason NVARCHAR(500),
    @TransferredByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @SourceFolioId UNIQUEIDENTIFIER, @Type INT, @Description NVARCHAR(500), @Amount DECIMAL(18,2),
            @GstAmount DECIMAL(18,2), @TotalAmount DECIMAL(18,2), @BusinessDate DATE;

    SELECT @SourceFolioId = FolioId, @Type = Type, @Description = Description, @Amount = Amount,
           @GstAmount = GstAmount, @TotalAmount = TotalAmount, @BusinessDate = BusinessDate
    FROM dbo.FolioTransactions WHERE Id = @SourceTransactionId;

    IF @SourceFolioId IS NULL
    BEGIN
        RAISERROR('Source folio transaction not found.', 16, 1);
        RETURN;
    END

    IF @Type NOT IN (0, 1) /* RoomCharge, Incidental */
    BEGIN
        RAISERROR('Only RoomCharge or Incidental transactions can be transferred.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.FolioTransactions WHERE ReversalOfTransactionId = @SourceTransactionId)
    BEGIN
        RAISERROR('This charge has already been transferred or reversed.', 16, 1);
        RETURN;
    END

    DECLARE @SourceReservationId UNIQUEIDENTIFIER = (SELECT ReservationId FROM dbo.Folios WHERE Id = @SourceFolioId);
    DECLARE @DestReservationId UNIQUEIDENTIFIER, @DestStatus INT;
    SELECT @DestReservationId = ReservationId, @DestStatus = Status FROM dbo.Folios WHERE Id = @DestinationFolioId;

    IF @DestReservationId IS NULL
    BEGIN
        RAISERROR('Destination folio not found.', 16, 1);
        RETURN;
    END

    IF @DestReservationId <> @SourceReservationId
    BEGIN
        RAISERROR('Charges can only be transferred between folios of the same stay.', 16, 1);
        RETURN;
    END

    IF @DestStatus <> 0 /* Open */
    BEGIN
        RAISERROR('Destination folio is not open.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;

    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    DECLARE @ReversalId UNIQUEIDENTIFIER = NEWID();
    DECLARE @NewChargeId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO dbo.FolioTransactions (Id, FolioId, Type, Description, Amount, GstAmount, TotalAmount, ReversalOfTransactionId, BusinessDate, PostedByUserId, CreatedAtUtc)
    VALUES (@ReversalId, @SourceFolioId, 8 /* Reversal */, CONCAT('Transfer out: ', @Reason), -@Amount, -@GstAmount, -@TotalAmount, @SourceTransactionId, @BusinessDate, @TransferredByUserId, @Now);

    INSERT INTO dbo.FolioTransactions (Id, FolioId, Type, Description, Amount, GstAmount, TotalAmount, BusinessDate, PostedByUserId, CreatedAtUtc)
    VALUES (@NewChargeId, @DestinationFolioId, @Type, CONCAT('Transferred in: ', @Reason), @Amount, @GstAmount, @TotalAmount, @BusinessDate, @TransferredByUserId, @Now);

    INSERT INTO dbo.FolioTaxLines (Id, FolioTransactionId, GstRuleId, HsnSac, TaxableValue, CgstRate, CgstAmount, SgstRate, SgstAmount, IgstRate, IgstAmount, CreatedAtUtc)
    SELECT NEWID(), @NewChargeId, GstRuleId, HsnSac, TaxableValue, CgstRate, CgstAmount, SgstRate, SgstAmount, IgstRate, IgstAmount, @Now
    FROM dbo.FolioTaxLines WHERE FolioTransactionId = @SourceTransactionId;

    UPDATE dbo.Folios SET Balance = Balance - @TotalAmount, UpdatedAtUtc = @Now WHERE Id = @SourceFolioId;
    UPDATE dbo.Folios SET Balance = Balance + @TotalAmount, UpdatedAtUtc = @Now WHERE Id = @DestinationFolioId;

    INSERT INTO dbo.FolioTransfers (Id, FromFolioId, ToFolioId, SourceReversalTransactionId, DestinationTransactionId, Amount, Reason, TransferredByUserId, TransferredAtUtc, CreatedAtUtc)
    VALUES (NEWID(), @SourceFolioId, @DestinationFolioId, @ReversalId, @NewChargeId, @TotalAmount, @Reason, @TransferredByUserId, @Now, @Now);

    INSERT INTO dbo.AuditLogs (Id, UserId, EntityType, EntityId, Action, DetailsJson, CreatedAtUtc)
    VALUES (NEWID(), @TransferredByUserId, 'FolioTransaction', CAST(@SourceTransactionId AS NVARCHAR(36)), 'TransferredCharge',
            (SELECT @SourceFolioId AS fromFolioId, @DestinationFolioId AS toFolioId, @Amount AS amount FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), @Now);

    COMMIT TRANSACTION;

    SELECT Id AS FolioId, Balance FROM dbo.Folios WHERE Id IN (@SourceFolioId, @DestinationFolioId);
END;
GO
