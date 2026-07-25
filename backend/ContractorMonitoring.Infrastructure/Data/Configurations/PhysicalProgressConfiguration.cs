using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

// Entity configuration for PhysicalProgress
public class PhysicalProgressConfiguration : IEntityTypeConfiguration<PhysicalProgress>
{
    public void Configure(EntityTypeBuilder<PhysicalProgress> builder)
    {
        builder.ToTable("PhysicalProgresses");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PlannedProgress)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(p => p.ActualProgress)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(p => p.ActivityDescription)
            .HasMaxLength(2000);

        builder.Property(p => p.Bottlenecks)
            .HasMaxLength(2000);

        builder.Property(p => p.MitigationPlan)
            .HasMaxLength(2000);

        builder.Property(p => p.SupportingDocument)
            .HasMaxLength(500);

        builder.Property(p => p.ReportedBy)
            .HasMaxLength(150);

        builder.Property(p => p.VerifiedBy)
            .HasMaxLength(150);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasMaxLength(20);

        // Relationships
        builder.HasOne(p => p.Project)
            .WithMany(p => p.PhysicalProgresses)
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.ProjectId);
        builder.HasIndex(p => p.ProgressDate);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.TenantId);

        // Soft delete filter
    }
}