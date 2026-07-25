using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

// Entity configuration for ContractorOfficeDetail
public class ContractorOfficeDetailConfiguration : IEntityTypeConfiguration<ContractorOfficeDetail>
{
    public void Configure(EntityTypeBuilder<ContractorOfficeDetail> builder)
    {
        builder.ToTable("ContractorOfficeDetails");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.TaxId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.PostalCode)
            .HasMaxLength(20);

        builder.Property(c => c.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(c => c.Website)
            .HasMaxLength(500);

        builder.Property(c => c.ContactPerson)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.ContactPersonPhone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.ContactPersonEmail)
            .HasMaxLength(256);

        builder.Property(c => c.LicenseNumber)
            .HasMaxLength(50);

        builder.Property(c => c.InsuranceDetails)
            .HasMaxLength(1000);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasMaxLength(20);

        // Indexes
        builder.HasIndex(c => c.RegistrationNumber).IsUnique();
        builder.HasIndex(c => c.TaxId).IsUnique();
        builder.HasIndex(c => c.Email);
        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => c.Status);

        // Soft delete filter
    }
}