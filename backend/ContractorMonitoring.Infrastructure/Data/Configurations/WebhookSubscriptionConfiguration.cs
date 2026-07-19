using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

public class WebhookSubscriptionConfiguration : IEntityTypeConfiguration<WebhookSubscription>
{
    public void Configure(EntityTypeBuilder<WebhookSubscription> builder)
    {
        builder.ToTable("WebhookSubscriptions");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Name).IsRequired().HasMaxLength(200);
        builder.Property(w => w.Url).IsRequired().HasMaxLength(500);
        builder.Property(w => w.Secret).IsRequired().HasMaxLength(200);
        builder.Property(w => w.Events).IsRequired().HasMaxLength(1000);

        builder.HasIndex(w => w.IsActive);
    }
}