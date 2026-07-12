namespace ContractorMonitoring.Application.DTOs.Reports;

public class ContractorWiseReportDto
{
    public string ContractorName { get; set; } = string.Empty;
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
    public decimal TotalContractValue { get; set; }
    public decimal TotalPaymentsReceived { get; set; }
    public decimal PendingPayments { get; set; }
    public List<ProjectSummaryDto> Projects { get; set; } = new();
}

public class ProjectSummaryDto
{
    public string ProjectName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public decimal Progress { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}