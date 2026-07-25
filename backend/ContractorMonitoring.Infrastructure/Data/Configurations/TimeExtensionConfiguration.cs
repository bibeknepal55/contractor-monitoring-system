using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

// Entity configuration for TimeExtension
public class TimeExtensionConfiguration : IEntityTypeConfiguration<TimeExtension>
{
    public void Configure(EntityTypeBuilder<TimeExtension> builder)
    {
        builder.ToTable("TimeExtensions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.ExtensionNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Reason)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(t => t.SupportingDocument)
            .HasMaxLength(500);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.ApprovedBy)
            .HasMaxLength(150);

        builder.Property(t => t.Remarks)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(t => t.Project)
            .WithMany(p => p.TimeExtensions)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(t => t.ExtensionNumber).IsUnique();
        builder.HasIndex(t => t.ProjectId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.RequestDate);
        builder.HasIndex(t => t.TenantId);

        // Soft delete filter
    }
}