using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayOps.Domain.Entities.Audit;
using StayOps.Domain.Entities.Identity;
using StayOps.Domain.Entities.NightAudit;
using StayOps.Domain.Entities.Pos;

namespace StayOps.Infrastructure.Persistence.Configurations;

public class PosOutletConfiguration : IEntityTypeConfiguration<PosOutlet>
{
    public void Configure(EntityTypeBuilder<PosOutlet> b)
    {
        b.ToTable("PosOutlets");
        b.Property(x => x.Code).HasMaxLength(20).IsRequired();
        b.HasIndex(x => new { x.HotelId, x.Code }).IsUnique();
    }
}

public class PosChargeConfiguration : IEntityTypeConfiguration<PosCharge>
{
    public void Configure(EntityTypeBuilder<PosCharge> b)
    {
        b.ToTable("PosCharges");
        b.Property(x => x.IdempotencyKey).HasMaxLength(150).IsRequired();
        // Outlet code + POS reference is the idempotency key: a duplicate POST returns this row's FolioTransactionId.
        b.HasIndex(x => x.IdempotencyKey).IsUnique();
    }
}

public class NightAuditRunConfiguration : IEntityTypeConfiguration<NightAuditRun>
{
    public void Configure(EntityTypeBuilder<NightAuditRun> b)
    {
        b.ToTable("NightAuditRuns");
        // One row per hotel/business-date; a Running row here is the exclusive lock sp_RunNightAudit checks.
        b.HasIndex(x => new { x.HotelId, x.BusinessDate }).IsUnique();

        b.HasMany(x => x.Exceptions).WithOne(x => x.NightAuditRun).HasForeignKey(x => x.NightAuditRunId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("AuditLogs");
        b.HasIndex(x => new { x.EntityType, x.EntityId });
        b.HasIndex(x => x.CreatedAtUtc);
    }
}

public class UserHotelAccessConfiguration : IEntityTypeConfiguration<UserHotelAccess>
{
    public void Configure(EntityTypeBuilder<UserHotelAccess> b)
    {
        b.ToTable("UserHotelAccesses");
        // Every hotel-scoped authorization check is "does (UserId, HotelId) exist" - must be fast and unique.
        b.HasIndex(x => new { x.UserId, x.HotelId }).IsUnique();
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("RefreshTokens");
        b.Property(x => x.TokenHash).HasMaxLength(200).IsRequired();
        b.HasIndex(x => x.TokenHash).IsUnique();
        b.HasIndex(x => x.UserId);
    }
}
