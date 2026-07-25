using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

// Entity configuration for Subcontractor
public class SubcontractorConfiguration : IEntityTypeConfiguration<Subcontractor>
{
    public void Configure(EntityTypeBuilder<Subcontractor> builder)
    {
        builder.ToTable("Subcontractors");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.ScopeOfWork)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(s => s.ContactPerson)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.ContactPhone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.ContactEmail)
            .HasMaxLength(256);

        builder.Property(s => s.ContractAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.PerformanceRating)
            .HasMaxLength(10);

        builder.Property(s => s.Remarks)
            .HasMaxLength(1000);

        builder.Property(s => s.LicenseNumber)
            .HasMaxLength(50);

        builder.Property(s => s.InsuranceDetails)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(s => s.Project)
            .WithMany()
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(s => s.ProjectId);
        builder.HasIndex(s => s.CompanyName);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.TenantId);

        // Soft delete filter
    }
}