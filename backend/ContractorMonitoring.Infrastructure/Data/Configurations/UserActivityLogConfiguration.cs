using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

// Database configuration for UserActivityLog with optimized indexes
// These indexes ensure fast filtering even with millions of records
public class UserActivityLogConfiguration : IEntityTypeConfiguration<UserActivityLog>
{
    public void Configure(EntityTypeBuilder<UserActivityLog> builder)
    {
        builder.ToTable("UserActivityLogs");

        builder.HasKey(u => u.Id);

        // User info fields
        builder.Property(u => u.UserName).HasMaxLength(200);
        builder.Property(u => u.UserEmail).HasMaxLength(256);
        builder.Property(u => u.UserRole).HasMaxLength(50);

        // Activity fields
        builder.Property(u => u.ActivityType).IsRequired().HasMaxLength(50);
        builder.Property(u => u.ModuleName).HasMaxLength(100);
        builder.Property(u => u.Action).HasMaxLength(500);
        builder.Property(u => u.Description).HasMaxLength(2000);

        // Request fields
        builder.Property(u => u.IpAddress).HasMaxLength(50);
        builder.Property(u => u.Location).HasMaxLength(200);
        builder.Property(u => u.DeviceInfo).HasMaxLength(500);
        builder.Property(u => u.RequestMethod).HasMaxLength(10);
        builder.Property(u => u.RequestUrl).HasMaxLength(500);
        builder.Property(u => u.RequestBody).HasColumnType("text");
        builder.Property(u => u.SessionId).HasMaxLength(100);

        // Relationship with User (set null on user delete to preserve audit trail)
        builder.HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.SetNull);

       // PERFORMANCE INDEXES - Critical for fast queries on large datasets
      
        // Single column indexes for direct filtering
        builder.HasIndex(u => u.UserId);
        builder.HasIndex(u => u.CreatedAt);
        builder.HasIndex(u => u.ActivityType);
        builder.HasIndex(u => u.ModuleName);
        builder.HasIndex(u => u.IpAddress);
        builder.HasIndex(u => u.ResponseStatus);
        builder.HasIndex(u => u.SessionId);
        builder.HasIndex(u => u.TenantId);

        // Composite indexes for common query patterns
        builder.HasIndex(u => new { u.UserId, u.CreatedAt });         // "Show me user X's activity sorted by time"
        builder.HasIndex(u => new { u.ActivityType, u.CreatedAt });   // "Show all failed logins today"
        builder.HasIndex(u => new { u.ModuleName, u.ActivityType });  // "Show all Create actions in Projects module"

        // No soft delete filter - audit logs should be immutable
        // Logs can only be purged manually by SuperAdmin
    }
}