using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data.Configurations;

// Entity configuration for ContractFinancialDetail
public class ContractFinancialDetailConfiguration : IEntityTypeConfiguration<ContractFinancialDetail>
{
    public void Configure(EntityTypeBuilder<ContractFinancialDetail> builder)
    {
        builder.ToTable("ContractFinancialDetails");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ContractAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.AdvancePayment)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.AdvancePaymentRecovered)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.TotalPaidAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.PendingPayment)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.PaymentTerms)
            .HasMaxLength(500);

        builder.Property(c => c.BankName)
            .HasMaxLength(200);

        builder.Property(c => c.BankAccountNumber)
            .HasMaxLength(50);

        builder.Property(c => c.BankBranch)
            .HasMaxLength(200);

        builder.Property(c => c.SwiftCode)
            .HasMaxLength(20);

        builder.Property(c => c.PaymentStatus)
            .HasMaxLength(20);

        // Relationships
        builder.HasOne(c => c.Project)
            .WithMany(p => p.ContractFinancialDetails)
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.ProjectId);
        builder.HasIndex(c => c.PaymentStatus);
        builder.HasIndex(c => c.TenantId);

        // Soft delete filter
    }
}