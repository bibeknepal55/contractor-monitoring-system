using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

public class ApprovalWorkflowConfiguration : IEntityTypeConfiguration<ApprovalWorkflow>
{
    public void Configure(EntityTypeBuilder<ApprovalWorkflow> builder)
    {
        builder.ToTable("ApprovalWorkflows");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.ModuleName).IsRequired().HasMaxLength(50);
        builder.Property(a => a.RecordTitle).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Comments).HasMaxLength(1000);
        builder.Property(a => a.RequestedBy).IsRequired().HasMaxLength(150);
        builder.Property(a => a.ApprovedBy).HasMaxLength(150);
        builder.Property(a => a.Status).IsRequired().HasMaxLength(20);
        builder.Property(a => a.PreviousStatus).HasMaxLength(20);
        builder.Property(a => a.NextApprover).HasMaxLength(150);

        builder.HasIndex(a => a.ModuleName);
        builder.HasIndex(a => a.RecordId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.TenantId);
    }
}