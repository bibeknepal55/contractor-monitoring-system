using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

public class RolePermissionHistoryConfiguration : IEntityTypeConfiguration<RolePermissionHistory>
{
    public void Configure(EntityTypeBuilder<RolePermissionHistory> builder)
    {
        builder.ToTable("RolePermissionHistories");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Action).IsRequired().HasMaxLength(20);
        builder.Property(r => r.ChangedBy).IsRequired().HasMaxLength(200);

        builder.HasOne(r => r.Role).WithMany().HasForeignKey(r => r.RoleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.Permission).WithMany().HasForeignKey(r => r.PermissionId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.RoleId);
        builder.HasIndex(r => r.ChangedAt);
    }
}