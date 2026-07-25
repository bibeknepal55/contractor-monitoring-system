using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

// Entity configuration for PerformanceBond
public class PerformanceBondConfiguration : IEntityTypeConfiguration<PerformanceBond>
{
    public void Configure(EntityTypeBuilder<PerformanceBond> builder)
    {
        builder.ToTable("PerformanceBonds");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.BondNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.BondAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.BondType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.IssuingBank)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.BondDocument)
            .HasMaxLength(500);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.Remarks)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(p => p.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.BondNumber).IsUnique();
        builder.HasIndex(p => p.ProjectId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.ExpiryDate);
        builder.HasIndex(p => p.TenantId);

        // Soft delete filter
    }
}