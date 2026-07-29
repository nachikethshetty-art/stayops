using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayOps.Domain.Entities.Reservations;

namespace StayOps.Infrastructure.Persistence.Configurations;

public class InventoryHoldConfiguration : IEntityTypeConfiguration<InventoryHold>
{
    public void Configure(EntityTypeBuilder<InventoryHold> b)
    {
        b.ToTable("InventoryHolds");
        b.Property(x => x.IdempotencyKey).HasMaxLength(100).IsRequired();

        // Idempotent hold creation: a retried request with the same key must not create a second hold.
        b.HasIndex(x => x.IdempotencyKey).IsUnique();

        // The core overbooking-prevention scan: "how many active/confirmed holds overlap this room type + date range".
        b.HasIndex(x => new { x.HotelId, x.RoomTypeId, x.Status, x.CheckInDate, x.CheckOutDate });
        // sp_ExpireInventoryHolds scans for active holds whose expiry has passed.
        b.HasIndex(x => new { x.Status, x.ExpiresAtUtc });
    }
}

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> b)
    {
        b.ToTable("Reservations");
        b.Property(x => x.ReservationNumber).HasMaxLength(30).IsRequired();
        b.Property(x => x.IdempotencyKey).HasMaxLength(100);

        b.HasIndex(x => x.ReservationNumber).IsUnique();
        b.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL");

        // Same overlap scan as holds, plus arrivals/departures dashboard queries by date + status.
        b.HasIndex(x => new { x.HotelId, x.RoomTypeId, x.Status, x.CheckInDate, x.CheckOutDate });
        b.HasIndex(x => new { x.HotelId, x.CheckInDate });
        b.HasIndex(x => new { x.HotelId, x.CheckOutDate });
    }
}

public class ReservationRoomAssignmentConfiguration : IEntityTypeConfiguration<ReservationRoomAssignment>
{
    public void Configure(EntityTypeBuilder<ReservationRoomAssignment> b)
    {
        b.ToTable("ReservationRoomAssignments");
        b.HasIndex(x => new { x.RoomId, x.UnassignedAtUtc });
        b.HasOne(x => x.Reservation).WithMany(x => x.RoomAssignments).HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ReservationNightRateConfiguration : IEntityTypeConfiguration<ReservationNightRate>
{
    public void Configure(EntityTypeBuilder<ReservationNightRate> b)
    {
        b.ToTable("ReservationNightRates");
        // Night Audit posts one room-charge row per stay/date - this is the lookup key for that.
        b.HasIndex(x => new { x.ReservationId, x.StayDate }).IsUnique();
        b.HasOne(x => x.Reservation).WithMany(x => x.NightRates).HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ReservationPolicySnapshotConfiguration : IEntityTypeConfiguration<ReservationPolicySnapshot>
{
    public void Configure(EntityTypeBuilder<ReservationPolicySnapshot> b)
    {
        b.ToTable("ReservationPolicySnapshots");
        b.HasIndex(x => x.ReservationId).IsUnique();
        b.HasOne(x => x.Reservation).WithOne(x => x.PolicySnapshot!)
            .HasForeignKey<ReservationPolicySnapshot>(x => x.ReservationId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ReservationPolicySnapshotRuleConfiguration : IEntityTypeConfiguration<ReservationPolicySnapshotRule>
{
    public void Configure(EntityTypeBuilder<ReservationPolicySnapshotRule> b)
    {
        b.ToTable("ReservationPolicySnapshotRules");
        b.HasOne(x => x.ReservationPolicySnapshot).WithMany(x => x.Rules)
            .HasForeignKey(x => x.ReservationPolicySnapshotId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CancellationConfiguration : IEntityTypeConfiguration<Cancellation>
{
    public void Configure(EntityTypeBuilder<Cancellation> b)
    {
        b.ToTable("Cancellations");
        b.HasIndex(x => x.ReservationId).IsUnique();
        b.HasOne(x => x.Reservation).WithMany().HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> b)
    {
        b.ToTable("Refunds");
        b.HasIndex(x => x.Status);
        b.HasOne(x => x.Cancellation).WithMany(x => x.Refunds).HasForeignKey(x => x.CancellationId).OnDelete(DeleteBehavior.Restrict);
    }
}
