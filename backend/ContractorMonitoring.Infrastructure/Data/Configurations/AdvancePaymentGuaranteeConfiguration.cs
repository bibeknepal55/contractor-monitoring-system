using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

// Entity configuration for AdvancePaymentGuarantee
public class AdvancePaymentGuaranteeConfiguration : IEntityTypeConfiguration<AdvancePaymentGuarantee>
{
    public void Configure(EntityTypeBuilder<AdvancePaymentGuarantee> builder)
    {
        builder.ToTable("AdvancePaymentGuarantees");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.GuaranteeNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.GuaranteeAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.IssuingBank)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.AdvanceAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.AmountRecovered)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Remarks)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(a => a.Project)
            .WithMany()
            .HasForeignKey(a => a.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(a => a.GuaranteeNumber).IsUnique();
        builder.HasIndex(a => a.ProjectId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.ExpiryDate);
        builder.HasIndex(a => a.TenantId);

        // Soft delete filter
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}