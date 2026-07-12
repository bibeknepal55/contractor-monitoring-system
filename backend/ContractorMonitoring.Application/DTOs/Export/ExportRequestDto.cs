namespace ContractorMonitoring.Application.DTOs.Export;

// Export request DTO
public class ExportRequestDto
{
    public string ReportType { get; set; } = string.Empty;
    public string Format { get; set; } = "excel"; // excel or pdf
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? ProjectId { get; set; }
}