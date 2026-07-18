namespace ContractorMonitoring.Application.DTOs.PerformanceBond;

public class PerformanceBondDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string BondNumber { get; set; } = string.Empty;
    public decimal BondAmount { get; set; }
    public string BondType { get; set; } = string.Empty;
    public string IssuingBank { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public string? BondDocument { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePerformanceBondDto
{
    public Guid ProjectId { get; set; }
    public string BondNumber { get; set; } = string.Empty;
    public decimal BondAmount { get; set; }
    public string BondType { get; set; } = string.Empty;
    public string IssuingBank { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string? Remarks { get; set; }
}

public class UpdatePerformanceBondDto
{
    public Guid ProjectId { get; set; }
    public string BondNumber { get; set; } = string.Empty;
    public decimal BondAmount { get; set; }
    public string BondType { get; set; } = string.Empty;
    public string IssuingBank { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}