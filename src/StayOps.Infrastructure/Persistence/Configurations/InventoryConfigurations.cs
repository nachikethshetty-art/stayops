using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayOps.Domain.Entities.Inventory;

namespace StayOps.Infrastructure.Persistence.Configurations;

public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    public void Configure(EntityTypeBuilder<RoomType> b)
    {
        b.ToTable("RoomTypes");
        b.Property(x => x.Code).HasMaxLength(20).IsRequired();
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();

        // Availability search groups by (HotelId, RoomTypeId) constantly - composite index supports that scan.
        b.HasIndex(x => new { x.HotelId, x.Code }).IsUnique();

        b.HasOne(x => x.Hotel).WithMany(x => x.RoomTypes).HasForeignKey(x => x.HotelId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> b)
    {
        b.ToTable("Rooms");
        b.Property(x => x.RoomNumber).HasMaxLength(20).IsRequired();

        b.HasIndex(x => new { x.HotelId, x.RoomNumber }).IsUnique();
        // Occupancy/night-audit/housekeeping queries constantly filter "rooms of this type, in this status".
        b.HasIndex(x => new { x.HotelId, x.RoomTypeId, x.Status });

        b.HasOne(x => x.Hotel).WithMany(x => x.Rooms).HasForeignKey(x => x.HotelId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.RoomType).WithMany(x => x.Rooms).HasForeignKey(x => x.RoomTypeId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RoomStatusHistoryConfiguration : IEntityTypeConfiguration<RoomStatusHistory>
{
    public void Configure(EntityTypeBuilder<RoomStatusHistory> b)
    {
        b.ToTable("RoomStatusHistories");
        b.HasIndex(x => new { x.RoomId, x.ChangedAtUtc });
        b.HasOne(x => x.Room).WithMany(x => x.StatusHistory).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RoomOutOfServicePeriodConfiguration : IEntityTypeConfiguration<RoomOutOfServicePeriod>
{
    public void Configure(EntityTypeBuilder<RoomOutOfServicePeriod> b)
    {
        b.ToTable("RoomOutOfServicePeriods");
        // Night Audit / occupancy report asks "is this room OOO/OOS on business date X" for every room every run.
        b.HasIndex(x => new { x.RoomId, x.StartDate, x.EndDate });
        b.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class HousekeepingTaskConfiguration : IEntityTypeConfiguration<HousekeepingTask>
{
    public void Configure(EntityTypeBuilder<HousekeepingTask> b)
    {
        b.ToTable("HousekeepingTasks");
        b.HasIndex(x => new { x.HotelId, x.Status });
        b.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
    }
}
