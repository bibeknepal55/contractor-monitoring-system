using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

public class PriceAdjustmentConfiguration : IEntityTypeConfiguration<PriceAdjustment>
{
    public void Configure(EntityTypeBuilder<PriceAdjustment> builder)
    {
        builder.ToTable("PriceAdjustments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.AdjustmentType).IsRequired().HasMaxLength(50);
        builder.Property(p => p.PreviousAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.NewAmount).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(p => p.PercentageChange).HasColumnType("decimal(8,2)");
        builder.Property(p => p.Currency).IsRequired().HasMaxLength(10);
        builder.Property(p => p.Reason).IsRequired().HasMaxLength(1000);
        builder.Property(p => p.ReferenceDocument).HasMaxLength(500);
        builder.Property(p => p.Status).IsRequired().HasMaxLength(20);
        builder.Property(p => p.ApprovedBy).HasMaxLength(150);
        builder.Property(p => p.RequestedBy).HasMaxLength(150);
        builder.Property(p => p.Remarks).HasMaxLength(1000);

        builder.HasOne(p => p.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.ProjectId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.AdjustmentDate);
        builder.HasIndex(p => p.TenantId);
    }
}