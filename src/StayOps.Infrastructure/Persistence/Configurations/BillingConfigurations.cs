using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayOps.Domain.Entities.Billing;

namespace StayOps.Infrastructure.Persistence.Configurations;

public class FolioConfiguration : IEntityTypeConfiguration<Folio>
{
    public void Configure(EntityTypeBuilder<Folio> b)
    {
        b.ToTable("Folios");
        // Check-in/checkout/POS-posting all ask "give me the open Guest folio for this reservation".
        b.HasIndex(x => new { x.ReservationId, x.Type });
        b.HasIndex(x => new { x.ReservationId, x.Status });
    }
}

public class FolioTransactionConfiguration : IEntityTypeConfiguration<FolioTransaction>
{
    public void Configure(EntityTypeBuilder<FolioTransaction> b)
    {
        b.ToTable("FolioTransactions");
        b.HasIndex(x => new { x.FolioId, x.BusinessDate });

        // Enforces "post at most once" for Night Audit room charges (key = reservation+date+ROOMCHARGE)
        // and POS charges (key = outlet code + POS reference) at the database level, not just in application code.
        b.HasIndex(x => x.UniquePostingKey).IsUnique().HasFilter("[UniquePostingKey] IS NOT NULL");

        b.HasOne(x => x.Folio).WithMany(x => x.Transactions).HasForeignKey(x => x.FolioId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class FolioTaxLineConfiguration : IEntityTypeConfiguration<FolioTaxLine>
{
    public void Configure(EntityTypeBuilder<FolioTaxLine> b)
    {
        b.ToTable("FolioTaxLines");
        b.HasOne(x => x.FolioTransaction).WithMany(x => x.TaxLines).HasForeignKey(x => x.FolioTransactionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class FolioTransferConfiguration : IEntityTypeConfiguration<FolioTransfer>
{
    public void Configure(EntityTypeBuilder<FolioTransfer> b)
    {
        b.ToTable("FolioTransfers");
        b.HasIndex(x => x.FromFolioId);
        b.HasIndex(x => x.ToFolioId);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("Payments");
        b.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL");
        b.HasIndex(x => x.FolioId);
        b.HasIndex(x => x.ReservationId);
    }
}

public class GstRuleConfiguration : IEntityTypeConfiguration<GstRule>
{
    public void Configure(EntityTypeBuilder<GstRule> b)
    {
        b.ToTable("GstRules");
        b.Property(x => x.HsnSac).HasMaxLength(20).IsRequired();
        // GST resolution is "category + amount slab + effective date", evaluated on every charge posted.
        b.HasIndex(x => new { x.ChargeCategory, x.EffectiveFrom, x.EffectiveTo });
    }
}

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> b)
    {
        b.ToTable("Invoices");
        b.Property(x => x.InvoiceNumber).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.InvoiceNumber).IsUnique();
        b.HasIndex(x => x.ReservationId);

        b.HasMany(x => x.Lines).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
    }
}
