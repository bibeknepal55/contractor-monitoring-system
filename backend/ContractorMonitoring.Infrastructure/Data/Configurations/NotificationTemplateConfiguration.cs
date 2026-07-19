using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.EventType).IsRequired().HasMaxLength(100);
        builder.Property(n => n.Subject).IsRequired().HasMaxLength(500);
        builder.Property(n => n.BodyTemplate).IsRequired().HasColumnType("text");
        builder.HasIndex(n => n.EventType).IsUnique();
    }
}