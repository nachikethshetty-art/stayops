using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayOps.Domain.Entities.CancellationPolicies;
using StayOps.Domain.Entities.Rates;

namespace StayOps.Infrastructure.Persistence.Configurations;

public class RatePlanConfiguration : IEntityTypeConfiguration<RatePlan>
{
    public void Configure(EntityTypeBuilder<RatePlan> b)
    {
        b.ToTable("RatePlans");
        b.Property(x => x.Code).HasMaxLength(20).IsRequired();
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.HasIndex(x => new { x.HotelId, x.Code }).IsUnique();

        b.HasOne(x => x.CancellationPolicy).WithMany().HasForeignKey(x => x.CancellationPolicyId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RatePlanPriceConfiguration : IEntityTypeConfiguration<RatePlanPrice>
{
    public void Configure(EntityTypeBuilder<RatePlanPrice> b)
    {
        b.ToTable("RatePlanPrices");
        // sp_SearchAvailableRoomTypes resolves price by exactly this key for every candidate date.
        b.HasIndex(x => new { x.RatePlanId, x.RoomTypeId, x.Occupancy, x.EffectiveFrom, x.EffectiveTo });

        b.HasOne(x => x.RatePlan).WithMany(x => x.Prices).HasForeignKey(x => x.RatePlanId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CancellationPolicyConfiguration : IEntityTypeConfiguration<CancellationPolicy>
{
    public void Configure(EntityTypeBuilder<CancellationPolicy> b)
    {
        b.ToTable("CancellationPolicies");
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.HasIndex(x => x.HotelId);
    }
}

public class CancellationPolicyRuleConfiguration : IEntityTypeConfiguration<CancellationPolicyRule>
{
    public void Configure(EntityTypeBuilder<CancellationPolicyRule> b)
    {
        b.ToTable("CancellationPolicyRules");
        b.HasIndex(x => new { x.CancellationPolicyId, x.SortOrder });
        b.HasOne(x => x.CancellationPolicy).WithMany(x => x.Rules).HasForeignKey(x => x.CancellationPolicyId).OnDelete(DeleteBehavior.Cascade);
    }
}
