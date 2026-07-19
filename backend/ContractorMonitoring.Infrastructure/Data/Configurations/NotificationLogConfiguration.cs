using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("NotificationLogs");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Type).IsRequired().HasMaxLength(20);
        builder.Property(n => n.EventType).IsRequired().HasMaxLength(100);
        builder.Property(n => n.Subject).HasMaxLength(500);
        builder.Property(n => n.Body).HasColumnType("text");
        builder.Property(n => n.Status).IsRequired().HasMaxLength(20);

        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.SentAt);
    }
}