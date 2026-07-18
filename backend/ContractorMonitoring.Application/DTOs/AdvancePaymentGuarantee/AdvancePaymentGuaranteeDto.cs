namespace ContractorMonitoring.Application.DTOs.AdvancePaymentGuarantee;

public class AdvancePaymentGuaranteeDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string GuaranteeNumber { get; set; } = string.Empty;
    public decimal GuaranteeAmount { get; set; }
    public string IssuingBank { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal AdvanceAmount { get; set; }
    public decimal AmountRecovered { get; set; }
    public decimal BalanceAmount { get; set; }
    public DateTime? LastRecoveryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAdvancePaymentGuaranteeDto
{
    public Guid ProjectId { get; set; }
    public string GuaranteeNumber { get; set; } = string.Empty;
    public decimal GuaranteeAmount { get; set; }
    public string IssuingBank { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal AdvanceAmount { get; set; }
    public string? Remarks { get; set; }
}

public class UpdateAdvancePaymentGuaranteeDto
{
    public Guid ProjectId { get; set; }
    public string GuaranteeNumber { get; set; } = string.Empty;
    public decimal GuaranteeAmount { get; set; }
    public string IssuingBank { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal AdvanceAmount { get; set; }
    public decimal AmountRecovered { get; set; }
    public DateTime? LastRecoveryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}