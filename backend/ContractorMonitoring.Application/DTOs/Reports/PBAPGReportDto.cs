namespace ContractorMonitoring.Application.DTOs.Reports;

public class PBAPGReportDto
{
    public string ProjectName { get; set; } = string.Empty;
    public string BondNumber { get; set; } = string.Empty;
    public string BondType { get; set; } = string.Empty;
    public decimal BondAmount { get; set; }
    public string IssuingBank { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsExpired { get; set; }
    public bool IsExpiringSoon { get; set; }
}