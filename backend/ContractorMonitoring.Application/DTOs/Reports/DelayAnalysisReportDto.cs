namespace ContractorMonitoring.Application.DTOs.Reports;

public class DelayAnalysisReportDto
{
    public string ProjectName { get; set; } = string.Empty;
    public string DelayCategory { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DelayStartDate { get; set; }
    public DateTime? DelayEndDate { get; set; }
    public int DelayDays { get; set; }
    public string ImpactLevel { get; set; } = string.Empty;
    public string ResponsibleParty { get; set; } = string.Empty;
    public string MitigationAction { get; set; } = string.Empty;
}

public class DelayReportDto
{
    public string DelayCategory { get; set; } = string.Empty;
    public int DelayDays { get; set; }
    public string ImpactLevel { get; set; } = string.Empty;
}