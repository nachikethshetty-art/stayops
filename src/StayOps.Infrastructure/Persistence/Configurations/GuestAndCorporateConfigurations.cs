using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayOps.Domain.Entities.Corporate;
using StayOps.Domain.Entities.Guests;

namespace StayOps.Infrastructure.Persistence.Configurations;

public class GuestConfiguration : IEntityTypeConfiguration<Guest>
{
    public void Configure(EntityTypeBuilder<Guest> b)
    {
        b.ToTable("Guests");
        b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.Phone).HasMaxLength(20);

        // Reception looks guests up by phone/email at booking time.
        b.HasIndex(x => x.Phone);
        b.HasIndex(x => x.Email);
    }
}

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> b)
    {
        b.ToTable("Companies");
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Gstin).HasMaxLength(15);
        b.HasIndex(x => x.Gstin);
    }
}

public class CorporateRateContractConfiguration : IEntityTypeConfiguration<CorporateRateContract>
{
    public void Configure(EntityTypeBuilder<CorporateRateContract> b)
    {
        b.ToTable("CorporateRateContracts");
        // Rate resolution's first lookup: "does this company have an active contract for this hotel today".
        b.HasIndex(x => new { x.CompanyId, x.HotelId, x.IsActive });
        b.HasOne(x => x.Company).WithMany(x => x.Contracts).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TravelAgentConfiguration : IEntityTypeConfiguration<TravelAgent>
{
    public void Configure(EntityTypeBuilder<TravelAgent> b)
    {
        b.ToTable("TravelAgents");
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
    }
}

public class AgentRateContractConfiguration : IEntityTypeConfiguration<AgentRateContract>
{
    public void Configure(EntityTypeBuilder<AgentRateContract> b)
    {
        b.ToTable("AgentRateContracts");
        b.HasIndex(x => new { x.TravelAgentId, x.HotelId, x.IsActive });
        b.HasOne(x => x.TravelAgent).WithMany(x => x.Contracts).HasForeignKey(x => x.TravelAgentId).OnDelete(DeleteBehavior.Restrict);
    }
}
