using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

// Entity configuration for PhotoMonitoring
public class PhotoMonitoringConfiguration : IEntityTypeConfiguration<PhotoMonitoring>
{
    public void Configure(EntityTypeBuilder<PhotoMonitoring> builder)
    {
        builder.ToTable("PhotoMonitorings");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.PhotoPath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Location)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Direction)
            .HasMaxLength(20);

        builder.Property(p => p.PhotoType)
            .HasMaxLength(50);

        builder.Property(p => p.Tags)
            .HasMaxLength(500);

        builder.Property(p => p.UploadedBy)
            .HasMaxLength(150);

        // Relationships
        builder.HasOne(p => p.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.ProjectId);
        builder.HasIndex(p => p.PhotoDate);
        builder.HasIndex(p => p.PhotoType);
        builder.HasIndex(p => p.TenantId);

        // Soft delete filter
    }
}