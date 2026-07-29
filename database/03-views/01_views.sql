/*
    Reporting/read-model views. These do not encapsulate business rules that change financial or
    inventory state - only convenience joins used by Dapper-backed list/report endpoints and by
    stored procedures that would otherwise repeat the same join.
    Idempotent: CREATE OR ALTER.
*/

CREATE OR ALTER VIEW dbo.vw_ReservationSummary
AS
SELECT
    r.Id                    AS ReservationId,
    r.HotelId,
    r.ReservationNumber,
    r.Status,
    r.Source,
    r.CheckInDate,
    r.CheckOutDate,
    r.RoomsBooked,
    r.Adults,
    r.Children,
    g.Id                    AS GuestId,
    g.FirstName + ' ' + g.LastName AS GuestName,
    g.Phone                 AS GuestPhone,
    g.Email                 AS GuestEmail,
    rt.Id                   AS RoomTypeId,
    rt.Name                 AS RoomTypeName,
    rp.Id                   AS RatePlanId,
    rp.Name                 AS RatePlanName,
    r.CompanyId,
    c.Name                  AS CompanyName,
    r.CreatedAtUtc
FROM dbo.Reservations r
JOIN dbo.Guests g       ON g.Id = r.GuestId
JOIN dbo.RoomTypes rt   ON rt.Id = r.RoomTypeId
JOIN dbo.RatePlans rp   ON rp.Id = r.RatePlanId
LEFT JOIN dbo.Companies c ON c.Id = r.CompanyId;
GO

CREATE OR ALTER VIEW dbo.vw_FolioBalanceReconciliation
AS
SELECT
    f.Id            AS FolioId,
    f.ReservationId,
    f.Type           AS FolioType,
    f.Status         AS FolioStatus,
    f.Balance        AS StoredBalance,
    ISNULL(SUM(ft.TotalAmount), 0) AS ComputedBalance
FROM dbo.Folios f
LEFT JOIN dbo.FolioTransactions ft ON ft.FolioId = f.Id
GROUP BY f.Id, f.ReservationId, f.Type, f.Status, f.Balance;
GO

-- Rooms that count as sellable/occupiable "today" from a pure room-status point of view (OOO date
-- ranges are date-scoped and therefore evaluated separately in sp_GetOccupancyReport, not here).
CREATE OR ALTER VIEW dbo.vw_ActiveRoomInventory
AS
SELECT
    ro.Id        AS RoomId,
    ro.HotelId,
    ro.RoomTypeId,
    ro.RoomNumber,
    ro.Status,
    ro.IsActive
FROM dbo.Rooms ro
WHERE ro.IsActive = 1;
GO
