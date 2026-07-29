using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StayOps.Application.Common.Interfaces;
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
using StayOps.Infrastructure.Identity;

namespace StayOps.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IApplicationDbContext
{
    public DbSet<HotelGroup> HotelGroups => Set<HotelGroup>();
    public DbSet<Hotel> Hotels => Set<Hotel>();

    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomStatusHistory> RoomStatusHistories => Set<RoomStatusHistory>();
    public DbSet<RoomOutOfServicePeriod> RoomOutOfServicePeriods => Set<RoomOutOfServicePeriod>();
    public DbSet<HousekeepingTask> HousekeepingTasks => Set<HousekeepingTask>();

    public DbSet<Guest> Guests => Set<Guest>();

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CorporateRateContract> CorporateRateContracts => Set<CorporateRateContract>();
    public DbSet<TravelAgent> TravelAgents => Set<TravelAgent>();
    public DbSet<AgentRateContract> AgentRateContracts => Set<AgentRateContract>();

    public DbSet<RatePlan> RatePlans => Set<RatePlan>();
    public DbSet<RatePlanPrice> RatePlanPrices => Set<RatePlanPrice>();

    public DbSet<CancellationPolicy> CancellationPolicies => Set<CancellationPolicy>();
    public DbSet<CancellationPolicyRule> CancellationPolicyRules => Set<CancellationPolicyRule>();

    public DbSet<InventoryHold> InventoryHolds => Set<InventoryHold>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationRoomAssignment> ReservationRoomAssignments => Set<ReservationRoomAssignment>();
    public DbSet<ReservationNightRate> ReservationNightRates => Set<ReservationNightRate>();
    public DbSet<ReservationPolicySnapshot> ReservationPolicySnapshots => Set<ReservationPolicySnapshot>();
    public DbSet<ReservationPolicySnapshotRule> ReservationPolicySnapshotRules => Set<ReservationPolicySnapshotRule>();
    public DbSet<Cancellation> Cancellations => Set<Cancellation>();
    public DbSet<Refund> Refunds => Set<Refund>();

    public DbSet<Folio> Folios => Set<Folio>();
    public DbSet<FolioTransaction> FolioTransactions => Set<FolioTransaction>();
    public DbSet<FolioTaxLine> FolioTaxLines => Set<FolioTaxLine>();
    public DbSet<FolioTransfer> FolioTransfers => Set<FolioTransfer>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<GstRule> GstRules => Set<GstRule>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();

    public DbSet<PosOutlet> PosOutlets => Set<PosOutlet>();
    public DbSet<PosCharge> PosCharges => Set<PosCharge>();

    public DbSet<NightAuditRun> NightAuditRuns => Set<NightAuditRun>();
    public DbSet<NightAuditException> NightAuditExceptions => Set<NightAuditException>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<UserHotelAccess> UserHotelAccesses => Set<UserHotelAccess>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // All money/percentage values are decimal - never floating point. 18,2 comfortably covers INR amounts.
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
