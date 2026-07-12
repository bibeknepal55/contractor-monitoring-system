namespace ContractorMonitoring.Application.DTOs.ContractFinancialDetail;

public class ContractFinancialDetailDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal ContractAmount { get; set; }
    public decimal? AdvancePayment { get; set; }
    public decimal? AdvancePaymentRecovered { get; set; }
    public decimal? TotalPaidAmount { get; set; }
    public decimal? PendingPayment { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = string.Empty;
    public int? PaymentMilestones { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankBranch { get; set; }
    public string? SwiftCode { get; set; }
    public DateTime? ContractSigningDate { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public string? PaymentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateContractFinancialDetailDto
{
    public Guid ProjectId { get; set; }
    public decimal ContractAmount { get; set; }
    public decimal? AdvancePayment { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentTerms { get; set; } = string.Empty;
    public int? PaymentMilestones { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankBranch { get; set; }
    public string? SwiftCode { get; set; }
    public DateTime? ContractSigningDate { get; set; }
}

public class UpdateContractFinancialDetailDto
{
    public decimal ContractAmount { get; set; }
    public decimal? AdvancePayment { get; set; }
    public decimal? AdvancePaymentRecovered { get; set; }
    public decimal? TotalPaidAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentTerms { get; set; } = string.Empty;
    public int? PaymentMilestones { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankBranch { get; set; }
    public string? SwiftCode { get; set; }
    public string? PaymentStatus { get; set; }
}