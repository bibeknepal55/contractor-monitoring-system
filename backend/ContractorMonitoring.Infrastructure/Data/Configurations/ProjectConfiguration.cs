using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

// Entity configuration for Project
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProjectCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.ProjectName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Budget)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.ActualCost)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Location)
            .HasMaxLength(500);

        builder.Property(p => p.ProjectManager)
            .HasMaxLength(150);

        builder.Property(p => p.ContactNumber)
            .HasMaxLength(20);

        builder.Property(p => p.ContractNumber)
            .HasMaxLength(100);

        builder.Property(p => p.Priority)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.ProgressPercentage)
            .HasColumnType("decimal(5,2)");

        // Relationships
        builder.HasOne(p => p.Contractor)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(p => p.ProjectCode)
            .IsUnique();

        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.ContractorId);
        builder.HasIndex(p => p.TenantId);
        builder.HasIndex(p => new { p.ProjectCode, p.TenantId }).IsUnique();

        // Soft delete filter
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}