/*
    sp_ExpireInventoryHolds - run on a fixed interval by InventoryHoldExpiryService (background
    hosted service). Expires any Active hold past its 10-minute ExpiresAtUtc and cancels the
    PendingPayment reservation riding along with it, releasing the room-type inventory.
    Repeat-safe: only rows still in Active status are touched, so re-running finds nothing to do.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ExpireInventoryHolds
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    DECLARE @ExpiredHoldIds TABLE (Id UNIQUEIDENTIFIER PRIMARY KEY);

    BEGIN TRANSACTION;

    UPDATE dbo.InventoryHolds
    SET Status = 2 /* Expired */, UpdatedAtUtc = @Now
    OUTPUT inserted.Id INTO @ExpiredHoldIds
    WHERE Status = 0 /* Active */ AND ExpiresAtUtc <= @Now;

    UPDATE res
    SET res.Status = 4 /* Cancelled */, res.UpdatedAtUtc = @Now
    FROM dbo.Reservations res
    JOIN @ExpiredHoldIds e ON e.Id = res.InventoryHoldId
    WHERE res.Status = 0 /* PendingPayment */;

    COMMIT TRANSACTION;

    SELECT COUNT(*) AS ExpiredCount FROM @ExpiredHoldIds;
END;
GO
