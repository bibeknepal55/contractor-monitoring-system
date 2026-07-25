using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

// Entity configuration for LabTest
public class LabTestConfiguration : IEntityTypeConfiguration<LabTest>
{
    public void Configure(EntityTypeBuilder<LabTest> builder)
    {
        builder.ToTable("LabTests");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.TestName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.TestCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.LabName)
            .HasMaxLength(200);

        builder.Property(l => l.TechnicianName)
            .HasMaxLength(150);

        builder.Property(l => l.TestStandard)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.TestResult)
            .HasMaxLength(20);

        builder.Property(l => l.Remarks)
            .HasMaxLength(1000);

        builder.Property(l => l.TestReportPath)
            .HasMaxLength(500);

        builder.Property(l => l.ParameterTested)
            .HasMaxLength(500);

        builder.Property(l => l.SpecificationLimit)
            .HasMaxLength(200);

        builder.Property(l => l.ActualValue)
            .HasMaxLength(200);

        // Relationships
        builder.HasOne(l => l.Project)
            .WithMany()
            .HasForeignKey(l => l.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(l => l.TestCode).IsUnique();
        builder.HasIndex(l => l.ProjectId);
        builder.HasIndex(l => l.Category);
        builder.HasIndex(l => l.TestDate);
        builder.HasIndex(l => l.TestResult);
        builder.HasIndex(l => l.TenantId);

        // Soft delete filter
    }
}