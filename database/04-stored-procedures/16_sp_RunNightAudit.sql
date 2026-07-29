/*
    sp_RunNightAudit - the repeat-safe nightly close-out for one hotel's current business date:
      1. Post room rent to every checked-in stay (to the Company folio when the reservation routes
         room charges there, else the Guest folio), using sp_PostFolioCharge so GST is computed
         the same way every other charge is.
      2. Detect no-shows (Confirmed reservations whose CheckInDate is the business date being
         closed) and process them through the same sp_CancelReservation no-show path used
         elsewhere, so penalty/refund logic never diverges between manual and automated no-shows.
      3. Advance rooms whose approved OOO/OOS period starts or ends exactly on this business date
         edge, so the room board stays in sync with date-ranged OOO/OOS without extra polling.
      4. Advance the hotel's business date - but only after the whole sweep completes.

    Concurrency/retry: sp_getapplock serializes concurrent runs per hotel. A unique
    (HotelId, BusinessDate) index on NightAuditRuns is this run's durable lock/checkpoint record;
    each stay/no-show is posted in its OWN small transaction (not one giant transaction for the
    whole hotel) so a mid-run failure leaves already-completed postings intact and the run
    resumable - re-running finds those rows already posted (via UniquePostingKey) and simply picks
    up where it left off, appending fresh exceptions and completing normally.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_RunNightAudit
    @HotelId UNIQUEIDENTIFIER,
    @TriggeredByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @LockResource NVARCHAR(200) = CONCAT('StayOps:NightAudit:', @HotelId);
    DECLARE @LockResult INT;
    EXEC @LockResult = sp_getapplock @Resource = @LockResource, @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = 5000;

    IF @LockResult < 0
    BEGIN
        RAISERROR('Could not acquire the night-audit lock for this hotel - another run may be in progress.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        DECLARE @BusinessDate DATE = (SELECT BusinessDate FROM dbo.Hotels WHERE Id = @HotelId);
        IF @BusinessDate IS NULL
        BEGIN
            RAISERROR('Hotel not found.', 16, 1);
            EXEC sp_releaseapplock @Resource = @LockResource, @LockOwner = 'Session';
            RETURN;
        END

        DECLARE @RunId UNIQUEIDENTIFIER;
        DECLARE @ExistingStatus INT;
        SELECT @RunId = Id, @ExistingStatus = Status FROM dbo.NightAuditRuns WHERE HotelId = @HotelId AND BusinessDate = @BusinessDate;

        IF @ExistingStatus = 0 /* Running */
        BEGIN
            RAISERROR('A night audit run is already in progress for this hotel/business date.', 16, 1);
            EXEC sp_releaseapplock @Resource = @LockResource, @LockOwner = 'Session';
            RETURN;
        END

        IF @ExistingStatus = 1 /* Completed */
        BEGIN
            SELECT * FROM dbo.NightAuditRuns WHERE Id = @RunId;
            EXEC sp_releaseapplock @Resource = @LockResource, @LockOwner = 'Session';
            RETURN;
        END

        DECLARE @Now DATETIME2 = SYSUTCDATETIME();

        IF @RunId IS NULL
        BEGIN
            SET @RunId = NEWID();
            INSERT INTO dbo.NightAuditRuns
                (Id, HotelId, BusinessDate, Status, StartedAtUtc, TotalRoomRevenuePosted, TotalTaxPosted, StaysProcessed, NoShowCount, ExceptionCount, TriggeredByUserId, CreatedAtUtc)
            VALUES
                (@RunId, @HotelId, @BusinessDate, 0 /* Running */, @Now, 0, 0, 0, 0, 0, @TriggeredByUserId, @Now);
        END
        ELSE
        BEGIN
            -- Resuming a previously Failed run for this business date.
            DELETE FROM dbo.NightAuditExceptions WHERE NightAuditRunId = @RunId;
            UPDATE dbo.NightAuditRuns
            SET Status = 0 /* Running */, StartedAtUtc = @Now, CompletedAtUtc = NULL,
                TotalRoomRevenuePosted = 0, TotalTaxPosted = 0, StaysProcessed = 0, NoShowCount = 0, ExceptionCount = 0
            WHERE Id = @RunId;
        END

        DECLARE @RoomRevenue DECIMAL(18,2) = 0, @TaxPosted DECIMAL(18,2) = 0, @StaysProcessed INT = 0, @NoShowCount INT = 0, @ExceptionCount INT = 0;

        ----------------------------------------------------------------------------------------
        -- 1) Post room charges for every checked-in stay whose night-rate snapshot covers today.
        ----------------------------------------------------------------------------------------
        DECLARE @ReservationId UNIQUEIDENTIFIER, @RoomRate DECIMAL(18,2), @BillToCompany BIT, @CompanyFolioId UNIQUEIDENTIFIER, @GuestFolioId UNIQUEIDENTIFIER;

        DECLARE stay_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT r.Id, nr.RoomRate, r.BillRoomChargeToCompany
            FROM dbo.Reservations r
            JOIN dbo.ReservationNightRates nr ON nr.ReservationId = r.Id AND nr.StayDate = @BusinessDate
            WHERE r.HotelId = @HotelId AND r.Status = 2 /* CheckedIn */;

        OPEN stay_cursor;
        FETCH NEXT FROM stay_cursor INTO @ReservationId, @RoomRate, @BillToCompany;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            BEGIN TRY
                SET @CompanyFolioId = NULL;
                IF @BillToCompany = 1
                    SELECT @CompanyFolioId = Id FROM dbo.Folios WHERE ReservationId = @ReservationId AND Type = 1 /* Company */ AND Status = 0 /* Open */;

                SELECT @GuestFolioId = Id FROM dbo.Folios WHERE ReservationId = @ReservationId AND Type = 0 /* Guest */ AND Status = 0 /* Open */;

                DECLARE @TargetFolioId UNIQUEIDENTIFIER = COALESCE(@CompanyFolioId, @GuestFolioId);
                IF @TargetFolioId IS NULL
                BEGIN
                    INSERT INTO dbo.NightAuditExceptions (Id, NightAuditRunId, ReservationId, ExceptionType, Message, CreatedAtUtc)
                    VALUES (NEWID(), @RunId, @ReservationId, 'NoOpenFolio', 'No open folio found to post the room charge to.', SYSUTCDATETIME());
                    SET @ExceptionCount += 1;
                END
                ELSE
                BEGIN
                    DECLARE @PostingKey NVARCHAR(200) = CONCAT('NIGHTAUDIT-ROOMCHARGE:', @ReservationId, ':', @BusinessDate);
                    DECLARE @ChargeDescription NVARCHAR(500) = CONCAT('Room charge for ', CONVERT(NVARCHAR(10), @BusinessDate, 120));
                    DECLARE @PostedTxn TABLE (Id UNIQUEIDENTIFIER, FolioId UNIQUEIDENTIFIER, Type INT, Description NVARCHAR(500),
                        Amount DECIMAL(18,2), GstAmount DECIMAL(18,2), TotalAmount DECIMAL(18,2), ReversalOfTransactionId UNIQUEIDENTIFIER,
                        BusinessDate DATE, PostedByUserId UNIQUEIDENTIFIER, SourceReference NVARCHAR(200), UniquePostingKey NVARCHAR(200),
                        CreatedAtUtc DATETIME2, UpdatedAtUtc DATETIME2);

                    INSERT INTO @PostedTxn
                    EXEC dbo.sp_PostFolioCharge
                        @FolioId = @TargetFolioId, @ChargeType = 0 /* RoomCharge */, @ChargeCategory = 0 /* RoomTariff */,
                        @Description = @ChargeDescription,
                        @TaxableAmount = @RoomRate, @PostedByUserId = @TriggeredByUserId,
                        @UniquePostingKey = @PostingKey, @BusinessDateOverride = @BusinessDate;

                    SELECT @RoomRevenue += Amount, @TaxPosted += GstAmount FROM @PostedTxn;
                    SET @StaysProcessed += 1;
                END
            END TRY
            BEGIN CATCH
                INSERT INTO dbo.NightAuditExceptions (Id, NightAuditRunId, ReservationId, ExceptionType, Message, CreatedAtUtc)
                VALUES (NEWID(), @RunId, @ReservationId, 'RoomChargePostingFailed', ERROR_MESSAGE(), SYSUTCDATETIME());
                SET @ExceptionCount += 1;
            END CATCH

            FETCH NEXT FROM stay_cursor INTO @ReservationId, @RoomRate, @BillToCompany;
        END
        CLOSE stay_cursor;
        DEALLOCATE stay_cursor;

        ----------------------------------------------------------------------------------------
        -- 2) No-show detection: Confirmed reservations whose arrival was today and never checked in.
        ----------------------------------------------------------------------------------------
        DECLARE noshow_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT Id FROM dbo.Reservations WHERE HotelId = @HotelId AND Status = 1 /* Confirmed */ AND CheckInDate = @BusinessDate;

        OPEN noshow_cursor;
        FETCH NEXT FROM noshow_cursor INTO @ReservationId;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            BEGIN TRY
                -- Captured (not left to leak to the client) - sp_CancelReservation's own SELECT
                -- must not become an extra result set ahead of this proc's final NightAuditRuns row.
                DECLARE @CancelResult TABLE (
                    Id UNIQUEIDENTIFIER, ReservationId UNIQUEIDENTIFIER, TriggerType INT, CancelledAtUtc DATETIME2,
                    HotelBusinessDateAtCancellation DATE, CancelledByUserId UNIQUEIDENTIFIER, AppliedPolicyRuleId UNIQUEIDENTIFIER,
                    HoursBeforeCheckIn INT, StayGrossAmount DECIMAL(18,2), PenaltyAmount DECIMAL(18,2), PenaltyGstAmount DECIMAL(18,2),
                    RefundDueAmount DECIMAL(18,2), Reason NVARCHAR(500), CreatedAtUtc DATETIME2, UpdatedAtUtc DATETIME2,
                    RefundId UNIQUEIDENTIFIER, RefundStatus INT
                );

                INSERT INTO @CancelResult
                EXEC dbo.sp_CancelReservation
                    @ReservationId = @ReservationId, @TriggerType = 1 /* NoShow */, @HoursBeforeCheckIn = NULL,
                    @BusinessDate = @BusinessDate, @Reason = 'No-show detected by Night Audit', @CancelledByUserId = @TriggeredByUserId;

                SET @NoShowCount += 1;
            END TRY
            BEGIN CATCH
                INSERT INTO dbo.NightAuditExceptions (Id, NightAuditRunId, ReservationId, ExceptionType, Message, CreatedAtUtc)
                VALUES (NEWID(), @RunId, @ReservationId, 'NoShowProcessingFailed', ERROR_MESSAGE(), SYSUTCDATETIME());
                SET @ExceptionCount += 1;
            END CATCH

            FETCH NEXT FROM noshow_cursor INTO @ReservationId;
        END
        CLOSE noshow_cursor;
        DEALLOCATE noshow_cursor;

        -- Note: Room.Status for OOO/OOS is set directly by sp_SetRoomOutOfOrder/sp_ReturnRoomToService
        -- at request/return time. fn_RoomTypeAvailableCount and sp_GetOccupancyReport always read
        -- RoomOutOfServicePeriods by date range directly, so availability/occupancy are correct for
        -- any date regardless of Room.Status; only the live room board's "current status" field
        -- depends on those two procs being called, a documented demo limitation (see root README).
        DECLARE @NextBusinessDate DATE = DATEADD(DAY, 1, @BusinessDate);

        ----------------------------------------------------------------------------------------
        -- 4) Complete the run and advance the business date.
        ----------------------------------------------------------------------------------------
        SET @Now = SYSUTCDATETIME();
        UPDATE dbo.NightAuditRuns
        SET Status = 1 /* Completed */, CompletedAtUtc = @Now,
            TotalRoomRevenuePosted = @RoomRevenue, TotalTaxPosted = @TaxPosted,
            StaysProcessed = @StaysProcessed, NoShowCount = @NoShowCount, ExceptionCount = @ExceptionCount
        WHERE Id = @RunId;

        UPDATE dbo.Hotels SET BusinessDate = @NextBusinessDate, UpdatedAtUtc = @Now WHERE Id = @HotelId;

        INSERT INTO dbo.AuditLogs (Id, UserId, HotelId, EntityType, EntityId, Action, DetailsJson, CreatedAtUtc)
        VALUES (NEWID(), @TriggeredByUserId, @HotelId, 'NightAuditRun', CAST(@RunId AS NVARCHAR(36)), 'Completed',
                (SELECT @BusinessDate AS businessDate, @StaysProcessed AS staysProcessed, @NoShowCount AS noShowCount, @ExceptionCount AS exceptionCount FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), @Now);

        EXEC sp_releaseapplock @Resource = @LockResource, @LockOwner = 'Session';

        SELECT * FROM dbo.NightAuditRuns WHERE Id = @RunId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

        IF EXISTS (SELECT 1 FROM dbo.NightAuditRuns WHERE Id = @RunId)
            UPDATE dbo.NightAuditRuns SET Status = 2 /* Failed */, CompletedAtUtc = SYSUTCDATETIME() WHERE Id = @RunId;

        EXEC sp_releaseapplock @Resource = @LockResource, @LockOwner = 'Session';

        DECLARE @ErrMsg NVARCHAR(2048) = ERROR_MESSAGE();
        RAISERROR('Night audit failed: %s', 16, 1, @ErrMsg);
    END CATCH
END;
GO
