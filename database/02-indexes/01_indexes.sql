/*
    Additional indexes layered on top of what EF Core migrations already create (see
    database/01-schema). These are covering/filtered indexes tuned for specific dashboard and
    report queries that are awkward to express through EF's Fluent API HasIndex() calls.
    Idempotent: safe to re-run.
*/

-- Dashboard "arrivals today" / "departures today" queries filter by hotel+status+date and then
-- immediately need GuestId/RoomTypeId - INCLUDE avoids a key lookup back into the clustered index.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reservations_Dashboard_Arrivals')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Reservations_Dashboard_Arrivals
        ON dbo.Reservations (HotelId, CheckInDate, Status)
        INCLUDE (GuestId, RoomTypeId, RoomsBooked, ReservationNumber);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reservations_Dashboard_Departures')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Reservations_Dashboard_Departures
        ON dbo.Reservations (HotelId, CheckOutDate, Status)
        INCLUDE (GuestId, RoomTypeId, RoomsBooked, ReservationNumber);
END;

-- sp_GetDailyRevenueAndGstReport and Night Audit both scan "all charge transactions posted on
-- business date X" grouped by type - this is the exact access pattern.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FolioTransactions_BusinessDate_Type')
BEGIN
    CREATE NONCLUSTERED INDEX IX_FolioTransactions_BusinessDate_Type
        ON dbo.FolioTransactions (BusinessDate, Type)
        INCLUDE (Amount, GstAmount, TotalAmount, FolioId);
END;

-- Occupancy report counts active (non-retired) rooms per hotel/status very frequently.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Rooms_Occupancy_Active')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Rooms_Occupancy_Active
        ON dbo.Rooms (HotelId, IsActive, Status)
        INCLUDE (RoomTypeId);
END;

-- GST rule resolution happens on every folio posting: category + still-active + effective-dated.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GstRules_Resolution')
BEGIN
    CREATE NONCLUSTERED INDEX IX_GstRules_Resolution
        ON dbo.GstRules (ChargeCategory, IsActive, EffectiveFrom, EffectiveTo)
        INCLUDE (MinAmount, MaxAmount, CgstRate, SgstRate, IgstRate, HotelId, HsnSac);
END;

-- Housekeeping board's default view: pending/in-progress tasks for a hotel, oldest first.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_HousekeepingTasks_Board')
BEGIN
    CREATE NONCLUSTERED INDEX IX_HousekeepingTasks_Board
        ON dbo.HousekeepingTasks (HotelId, Status, CreatedAtUtc)
        INCLUDE (RoomId, TaskType, AssignedToUserId);
END;
