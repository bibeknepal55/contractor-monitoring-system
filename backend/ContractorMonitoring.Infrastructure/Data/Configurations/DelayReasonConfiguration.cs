using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

// Entity configuration for DelayReason
public class DelayReasonConfiguration : IEntityTypeConfiguration<DelayReason>
{
    public void Configure(EntityTypeBuilder<DelayReason> builder)
    {
        builder.ToTable("DelayReasons");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.DelayCategory)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(d => d.ImpactLevel)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(d => d.ResponsibleParty)
            .HasMaxLength(50);

        builder.Property(d => d.MitigationAction)
            .HasMaxLength(2000);

        builder.Property(d => d.Remarks)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(d => d.Project)
            .WithMany()
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(d => d.ProjectId);
        builder.HasIndex(d => d.DelayCategory);
        builder.HasIndex(d => d.ImpactLevel);
        builder.HasIndex(d => d.DelayStartDate);
        builder.HasIndex(d => d.TenantId);

        // Soft delete filter
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}