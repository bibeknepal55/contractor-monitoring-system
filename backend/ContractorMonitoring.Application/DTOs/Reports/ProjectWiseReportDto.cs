using ContractorMonitoring.Application.DTOs.PhysicalProgress;

namespace ContractorMonitoring.Application.DTOs.Reports;

public class ProjectWiseReportDto
{
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public string ContractorName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public decimal ActualCost { get; set; }
    public decimal ProgressPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int TimeExtensions { get; set; }
    public int TotalDelays { get; set; }
    public List<PhysicalProgressDto> ProgressHistory { get; set; } = new();
    public List<DelayReportDto> Delays { get; set; } = new();
}