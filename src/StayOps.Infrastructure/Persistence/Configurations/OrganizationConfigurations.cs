using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayOps.Domain.Entities.Organization;

namespace StayOps.Infrastructure.Persistence.Configurations;

public class HotelGroupConfiguration : IEntityTypeConfiguration<HotelGroup>
{
    public void Configure(EntityTypeBuilder<HotelGroup> b)
    {
        b.ToTable("HotelGroups");
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
    }
}

public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> b)
    {
        b.ToTable("Hotels");
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Code).HasMaxLength(20).IsRequired();
        b.Property(x => x.StateCode).HasMaxLength(2).IsRequired();
        b.Property(x => x.Gstin).HasMaxLength(15).IsRequired();
        b.Property(x => x.TimeZoneId).HasMaxLength(64).IsRequired();

        // Every hotel lookup and every hotel-scoped query filters by Code or Id; Code must be unique for demo login/onboarding UX.
        b.HasIndex(x => x.Code).IsUnique();
        b.HasIndex(x => x.HotelGroupId);

        b.HasOne(x => x.HotelGroup)
            .WithMany(x => x.Hotels)
            .HasForeignKey(x => x.HotelGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
