using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Advance payment guarantee entity
public class AdvancePaymentGuarantee : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public string GuaranteeNumber { get; set; } = string.Empty;
    public decimal GuaranteeAmount { get; set; }
    public string IssuingBank { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal AdvanceAmount { get; set; }
    public decimal AmountRecovered { get; set; }
    public decimal BalanceAmount => AdvanceAmount - AmountRecovered;
    public DateTime? LastRecoveryDate { get; set; }
    public string Status { get; set; } = string.Empty; // Active, Recovered, Expired, Released
    public string? Remarks { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
}