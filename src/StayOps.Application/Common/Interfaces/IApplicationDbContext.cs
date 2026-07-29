using Microsoft.EntityFrameworkCore;
using StayOps.Domain.Entities.Audit;
using StayOps.Domain.Entities.Billing;
using StayOps.Domain.Entities.CancellationPolicies;
using StayOps.Domain.Entities.Corporate;
using StayOps.Domain.Entities.Guests;
using StayOps.Domain.Entities.Identity;
using StayOps.Domain.Entities.Inventory;
using StayOps.Domain.Entities.NightAudit;
using StayOps.Domain.Entities.Organization;
using StayOps.Domain.Entities.Pos;
using StayOps.Domain.Entities.Rates;
using StayOps.Domain.Entities.Reservations;

namespace StayOps.Application.Common.Interfaces;

/// <summary>
/// EF Core surface used by Application services for normal CRUD and change tracking.
/// Stored-procedure-driven transactional workflows (holds, night audit, folio posting, etc.)
/// go through Dapper via IDapperConnectionFactory instead, per the README's data-access split.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<HotelGroup> HotelGroups { get; }
    DbSet<Hotel> Hotels { get; }

    DbSet<RoomType> RoomTypes { get; }
    DbSet<Room> Rooms { get; }
    DbSet<RoomStatusHistory> RoomStatusHistories { get; }
    DbSet<RoomOutOfServicePeriod> RoomOutOfServicePeriods { get; }
    DbSet<HousekeepingTask> HousekeepingTasks { get; }

    DbSet<Guest> Guests { get; }

    DbSet<Company> Companies { get; }
    DbSet<CorporateRateContract> CorporateRateContracts { get; }
    DbSet<TravelAgent> TravelAgents { get; }
    DbSet<AgentRateContract> AgentRateContracts { get; }

    DbSet<RatePlan> RatePlans { get; }
    DbSet<RatePlanPrice> RatePlanPrices { get; }

    DbSet<CancellationPolicy> CancellationPolicies { get; }
    DbSet<CancellationPolicyRule> CancellationPolicyRules { get; }

    DbSet<InventoryHold> InventoryHolds { get; }
    DbSet<Reservation> Reservations { get; }
    DbSet<ReservationRoomAssignment> ReservationRoomAssignments { get; }
    DbSet<ReservationNightRate> ReservationNightRates { get; }
    DbSet<ReservationPolicySnapshot> ReservationPolicySnapshots { get; }
    DbSet<ReservationPolicySnapshotRule> ReservationPolicySnapshotRules { get; }
    DbSet<Cancellation> Cancellations { get; }
    DbSet<Refund> Refunds { get; }

    DbSet<Folio> Folios { get; }
    DbSet<FolioTransaction> FolioTransactions { get; }
    DbSet<FolioTaxLine> FolioTaxLines { get; }
    DbSet<FolioTransfer> FolioTransfers { get; }
    DbSet<Payment> Payments { get; }
    DbSet<GstRule> GstRules { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceLine> InvoiceLines { get; }

    DbSet<PosOutlet> PosOutlets { get; }
    DbSet<PosCharge> PosCharges { get; }

    DbSet<NightAuditRun> NightAuditRuns { get; }
    DbSet<NightAuditException> NightAuditExceptions { get; }

    DbSet<AuditLog> AuditLogs { get; }

    DbSet<UserHotelAccess> UserHotelAccesses { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
