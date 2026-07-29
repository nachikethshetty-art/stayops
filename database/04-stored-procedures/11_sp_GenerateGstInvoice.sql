/*
    sp_GenerateGstInvoice - generates a GST invoice for a folio from its not-yet-invoiced,
    not-reversed charge transactions (RoomCharge/Incidental). Re-running for the same folio after
    new charges are posted produces a supplementary invoice containing only the new lines -
    already-invoiced or reversed-out transactions are never included again.

    CGST+SGST is applied when the hotel's state equals the billed party's state; IGST otherwise.
    Rates/amounts are read from FolioTaxLines exactly as posted - never recalculated or hard-coded.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GenerateGstInvoice
    @FolioId UNIQUEIDENTIFIER,
    @CreatedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ReservationId UNIQUEIDENTIFIER, @FolioType INT, @OwnerCompanyId UNIQUEIDENTIFIER, @HotelId UNIQUEIDENTIFIER, @GuestId UNIQUEIDENTIFIER;
    SELECT @ReservationId = f.ReservationId, @FolioType = f.Type, @OwnerCompanyId = f.OwnerCompanyId, @HotelId = r.HotelId, @GuestId = r.GuestId
    FROM dbo.Folios f JOIN dbo.Reservations r ON r.Id = f.ReservationId
    WHERE f.Id = @FolioId;

    IF @ReservationId IS NULL
    BEGIN
        RAISERROR('Folio not found.', 16, 1);
        RETURN;
    END

    DECLARE @SupplierGstin NVARCHAR(15), @SupplierStateCode NVARCHAR(2);
    SELECT @SupplierGstin = Gstin, @SupplierStateCode = StateCode FROM dbo.Hotels WHERE Id = @HotelId;

    DECLARE @BilledPartyName NVARCHAR(200), @BilledPartyGstin NVARCHAR(15), @BilledPartyStateCode NVARCHAR(2);

    IF @OwnerCompanyId IS NOT NULL
        SELECT @BilledPartyName = Name, @BilledPartyGstin = Gstin, @BilledPartyStateCode = StateCode FROM dbo.Companies WHERE Id = @OwnerCompanyId;
    ELSE
        SELECT @BilledPartyName = CONCAT(FirstName, ' ', LastName), @BilledPartyGstin = Gstin, @BilledPartyStateCode = StateCode FROM dbo.Guests WHERE Id = @GuestId;

    IF @BilledPartyStateCode IS NULL SET @BilledPartyStateCode = @SupplierStateCode;
    DECLARE @IsInterState BIT = CASE WHEN @BilledPartyStateCode = @SupplierStateCode THEN 0 ELSE 1 END;

    DECLARE @Lines TABLE (
        FolioTransactionId UNIQUEIDENTIFIER, Description NVARCHAR(500), HsnSac NVARCHAR(20),
        TaxableValue DECIMAL(18,2), CgstRate DECIMAL(18,2), CgstAmount DECIMAL(18,2),
        SgstRate DECIMAL(18,2), SgstAmount DECIMAL(18,2), IgstRate DECIMAL(18,2), IgstAmount DECIMAL(18,2)
    );

    INSERT INTO @Lines
    SELECT ft.Id, ft.Description, tl.HsnSac, tl.TaxableValue,
           tl.CgstRate, CASE WHEN @IsInterState = 0 THEN tl.CgstAmount ELSE 0 END,
           tl.SgstRate, CASE WHEN @IsInterState = 0 THEN tl.SgstAmount ELSE 0 END,
           tl.IgstRate, CASE WHEN @IsInterState = 1 THEN tl.IgstAmount ELSE 0 END
    FROM dbo.FolioTransactions ft
    JOIN dbo.FolioTaxLines tl ON tl.FolioTransactionId = ft.Id
    WHERE ft.FolioId = @FolioId
      AND ft.Type IN (0, 1) /* RoomCharge, Incidental */
      AND ft.Id NOT IN (SELECT ReversalOfTransactionId FROM dbo.FolioTransactions WHERE ReversalOfTransactionId IS NOT NULL)
      AND ft.Id NOT IN (SELECT FolioTransactionId FROM dbo.InvoiceLines WHERE FolioTransactionId IS NOT NULL);

    IF NOT EXISTS (SELECT 1 FROM @Lines)
    BEGIN
        RAISERROR('No new, un-invoiced charges exist on this folio.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;

    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    DECLARE @InvoiceId UNIQUEIDENTIFIER = NEWID();
    DECLARE @InvoiceNumber NVARCHAR(30) = CONCAT('INV-', FORMAT(SYSUTCDATETIME(), 'yyyyMMdd'), '-', RIGHT(CONCAT('000000', NEXT VALUE FOR dbo.InvoiceNumberSeq), 6));

    DECLARE @TotalTaxable DECIMAL(18,2), @TotalCgst DECIMAL(18,2), @TotalSgst DECIMAL(18,2), @TotalIgst DECIMAL(18,2);
    SELECT @TotalTaxable = SUM(TaxableValue), @TotalCgst = SUM(CgstAmount), @TotalSgst = SUM(SgstAmount), @TotalIgst = SUM(IgstAmount)
    FROM @Lines;

    INSERT INTO dbo.Invoices
        (Id, ReservationId, FolioId, InvoiceNumber, InvoiceDate, SupplierGstin, SupplierStateCode,
         BilledPartyName, BilledPartyGstin, BilledPartyStateCode, PlaceOfSupplyStateCode, IsInterState,
         TotalTaxableValue, TotalCgst, TotalSgst, TotalIgst, TotalAmount, CreatedAtUtc)
    VALUES
        (@InvoiceId, @ReservationId, @FolioId, @InvoiceNumber, CAST(@Now AS DATE), @SupplierGstin, @SupplierStateCode,
         @BilledPartyName, @BilledPartyGstin, @BilledPartyStateCode, @SupplierStateCode, @IsInterState,
         @TotalTaxable, @TotalCgst, @TotalSgst, @TotalIgst, @TotalTaxable + @TotalCgst + @TotalSgst + @TotalIgst, @Now);

    INSERT INTO dbo.InvoiceLines
        (Id, InvoiceId, FolioTransactionId, Description, HsnSac, TaxableValue, CgstRate, CgstAmount, SgstRate, SgstAmount, IgstRate, IgstAmount, LineTotal, CreatedAtUtc)
    SELECT NEWID(), @InvoiceId, FolioTransactionId, Description, HsnSac, TaxableValue, CgstRate, CgstAmount, SgstRate, SgstAmount, IgstRate, IgstAmount,
           TaxableValue + CgstAmount + SgstAmount + IgstAmount, @Now
    FROM @Lines;

    INSERT INTO dbo.AuditLogs (Id, UserId, HotelId, EntityType, EntityId, Action, DetailsJson, CreatedAtUtc)
    VALUES (NEWID(), @CreatedByUserId, @HotelId, 'Invoice', CAST(@InvoiceId AS NVARCHAR(36)), 'Generated',
            (SELECT @InvoiceNumber AS invoiceNumber, @FolioId AS folioId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), @Now);

    COMMIT TRANSACTION;

    SELECT * FROM dbo.Invoices WHERE Id = @InvoiceId;
END;
GO
