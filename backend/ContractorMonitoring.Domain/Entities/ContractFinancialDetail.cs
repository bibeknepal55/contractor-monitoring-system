using ContractorMonitoring.Domain.Entities.Base;
using static ContractorMonitoring.Domain.Constants.Permissions;

namespace ContractorMonitoring.Domain.Entities;

// Contract financial details entity
public class ContractFinancialDetail : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public decimal ContractAmount { get; set; }
    public decimal? AdvancePayment { get; set; }
    public decimal? AdvancePaymentRecovered { get; set; }
    public decimal? TotalPaidAmount { get; set; }
    public decimal? PendingPayment { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentTerms { get; set; } = string.Empty;
    public int? PaymentMilestones { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankBranch { get; set; }
    public string? SwiftCode { get; set; }
    public DateTime? ContractSigningDate { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public string? PaymentStatus { get; set; } // Paid, Pending, Overdue, Partial

    // Navigation properties
    public Project Project { get; set; } = null!;
    public ICollection<PriceAdjustment> PriceAdjustments { get; set; } = new List<PriceAdjustment>();
}