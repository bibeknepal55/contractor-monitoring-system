using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

public class RevokedTokenConfiguration : IEntityTypeConfiguration<RevokedToken>
{
    public void Configure(EntityTypeBuilder<RevokedToken> builder)
    {
        builder.ToTable("RevokedTokens");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Jti).IsRequired().HasMaxLength(500);
        builder.Property(r => r.RevokedBy).HasMaxLength(200);
        builder.Property(r => r.Reason).HasMaxLength(500);

        builder.HasIndex(r => r.Jti).IsUnique();
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.ExpiresAt); // For cleanup job
    }
}