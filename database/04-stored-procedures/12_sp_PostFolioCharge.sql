/*
    sp_PostFolioCharge - shared charge-posting primitive (not itself one of the README's named
    procedures, but the single implementation behind three of them: manual incidental charges
    from the folio workspace, sp_PostPosChargeToFolio, and the nightly room-charge posting in
    sp_RunNightAudit). Resolves GST via fn_ResolveGstRule, posts the charge + tax line, and updates
    the folio balance, all in one transaction. Idempotent via the optional @UniquePostingKey,
    enforced by a database-level filtered unique index (see FolioTransactionConfiguration).
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_PostFolioCharge
    @FolioId UNIQUEIDENTIFIER,
    @ChargeType INT, -- 0 = RoomCharge, 1 = Incidental
    @ChargeCategory INT, -- GstChargeCategory: 0 = RoomTariff, 1 = FoodAndBeverage, 2 = OtherServices
    @Description NVARCHAR(500),
    @TaxableAmount DECIMAL(18,2),
    @PostedByUserId UNIQUEIDENTIFIER = NULL,
    @SourceReference NVARCHAR(200) = NULL,
    @UniquePostingKey NVARCHAR(200) = NULL,
    @BusinessDateOverride DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @UniquePostingKey IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.FolioTransactions WHERE UniquePostingKey = @UniquePostingKey)
    BEGIN
        SELECT * FROM dbo.FolioTransactions WHERE UniquePostingKey = @UniquePostingKey;
        RETURN;
    END

    DECLARE @HotelId UNIQUEIDENTIFIER, @Status INT;
    SELECT @HotelId = r.HotelId, @Status = f.Status
    FROM dbo.Folios f JOIN dbo.Reservations r ON r.Id = f.ReservationId
    WHERE f.Id = @FolioId;

    IF @HotelId IS NULL
    BEGIN
        RAISERROR('Folio not found.', 16, 1);
        RETURN;
    END

    IF @Status <> 0 /* Open */
    BEGIN
        RAISERROR('Cannot post a charge to a closed folio.', 16, 1);
        RETURN;
    END

    IF @TaxableAmount <= 0
    BEGIN
        RAISERROR('Charge amount must be positive.', 16, 1);
        RETURN;
    END

    DECLARE @BusinessDate DATE = COALESCE(@BusinessDateOverride, (SELECT BusinessDate FROM dbo.Hotels WHERE Id = @HotelId));

    DECLARE @GstRuleId UNIQUEIDENTIFIER, @CgstRate DECIMAL(18,2), @SgstRate DECIMAL(18,2), @IgstRate DECIMAL(18,2), @HsnSac NVARCHAR(20);
    SELECT @GstRuleId = GstRuleId, @CgstRate = CgstRate, @SgstRate = SgstRate, @IgstRate = IgstRate, @HsnSac = HsnSac
    FROM dbo.fn_ResolveGstRule(@HotelId, @ChargeCategory, @TaxableAmount, @BusinessDate);

    IF @GstRuleId IS NULL
    BEGIN
        RAISERROR('No active GST rule matched this charge - check GST rule configuration.', 16, 1);
        RETURN;
    END

    -- By design/seed data, IGST always equals CGST+SGST combined for the same slab (the standard
    -- Indian GST structure), so the total tax amount posted to the folio ledger is correct either
    -- way; only the CGST+SGST-vs-IGST *breakdown* shown on the invoice depends on the bill-to
    -- party's state, decided later at invoice-generation time (see sp_GenerateGstInvoice).
    DECLARE @CgstAmount DECIMAL(18,2) = ROUND(@TaxableAmount * @CgstRate / 100.0, 2);
    DECLARE @SgstAmount DECIMAL(18,2) = ROUND(@TaxableAmount * @SgstRate / 100.0, 2);
    DECLARE @IgstAmount DECIMAL(18,2) = ROUND(@TaxableAmount * @IgstRate / 100.0, 2);
    DECLARE @GstAmount DECIMAL(18,2) = @CgstAmount + @SgstAmount;

    BEGIN TRANSACTION;

    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    DECLARE @TxnId UNIQUEIDENTIFIER = NEWID();
    DECLARE @TotalAmount DECIMAL(18,2) = @TaxableAmount + @GstAmount;

    INSERT INTO dbo.FolioTransactions (Id, FolioId, Type, Description, Amount, GstAmount, TotalAmount, BusinessDate, PostedByUserId, SourceReference, UniquePostingKey, CreatedAtUtc)
    VALUES (@TxnId, @FolioId, @ChargeType, @Description, @TaxableAmount, @GstAmount, @TotalAmount, @BusinessDate, @PostedByUserId, @SourceReference, @UniquePostingKey, @Now);

    INSERT INTO dbo.FolioTaxLines (Id, FolioTransactionId, GstRuleId, HsnSac, TaxableValue, CgstRate, CgstAmount, SgstRate, SgstAmount, IgstRate, IgstAmount, CreatedAtUtc)
    VALUES (NEWID(), @TxnId, @GstRuleId, @HsnSac, @TaxableAmount, @CgstRate, @CgstAmount, @SgstRate, @SgstAmount, @IgstRate, @IgstAmount, @Now);

    UPDATE dbo.Folios SET Balance = Balance + @TotalAmount, UpdatedAtUtc = @Now WHERE Id = @FolioId;

    COMMIT TRANSACTION;

    SELECT * FROM dbo.FolioTransactions WHERE Id = @TxnId;
END;
GO
