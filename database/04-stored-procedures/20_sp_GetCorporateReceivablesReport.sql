/*
    sp_GetCorporateReceivablesReport - outstanding balance per company across all of that
    company's folios at this hotel (open folios still accruing, plus closed-but-unpaid ones
    awaiting collection). Powers the frontend's corporate receivables report.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetCorporateReceivablesReport
    @HotelId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.Id AS CompanyId,
        c.Name AS CompanyName,
        c.Gstin,
        c.CreditLimit,
        COUNT(DISTINCT f.Id) AS OpenFolioCount,
        SUM(f.Balance) AS TotalOutstandingBalance
    FROM dbo.Folios f
    JOIN dbo.Reservations r ON r.Id = f.ReservationId
    JOIN dbo.Companies c ON c.Id = f.OwnerCompanyId
    WHERE r.HotelId = @HotelId
      AND f.Type = 1 /* Company */
      AND f.Balance > 0
    GROUP BY c.Id, c.Name, c.Gstin, c.CreditLimit
    ORDER BY TotalOutstandingBalance DESC;
END;
GO
