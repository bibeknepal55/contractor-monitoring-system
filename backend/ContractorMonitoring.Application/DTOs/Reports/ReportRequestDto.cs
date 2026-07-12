namespace ContractorMonitoring.Application.DTOs.Reports;

// Report request with filters
public class ReportRequestDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? ContractorId { get; set; }
    public string? Status { get; set; }
    public string? ReportType { get; set; }
    public string? Format { get; set; } = "json"; // json, pdf, excel
}