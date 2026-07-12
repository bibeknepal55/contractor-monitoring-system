using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

// Entity configuration for RawMaterial
public class RawMaterialConfiguration : IEntityTypeConfiguration<RawMaterial>
{
    public void Configure(EntityTypeBuilder<RawMaterial> builder)
    {
        builder.ToTable("RawMaterials");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.MaterialName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.MaterialCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.QuantityOrdered)
            .IsRequired()
            .HasColumnType("decimal(18,4)");

        builder.Property(r => r.QuantityReceived)
            .IsRequired()
            .HasColumnType("decimal(18,4)");

        builder.Property(r => r.QuantityUsed)
            .IsRequired()
            .HasColumnType("decimal(18,4)");

        builder.Property(r => r.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.SupplierName)
            .HasMaxLength(200);

        builder.Property(r => r.QualityCertificate)
            .HasMaxLength(500);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasMaxLength(20);

        // Relationships
        builder.HasOne(r => r.Project)
            .WithMany()
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => r.MaterialCode).IsUnique();
        builder.HasIndex(r => r.ProjectId);
        builder.HasIndex(r => r.Category);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.TenantId);

        // Soft delete filter
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}