namespace ContractorMonitoring.Application.DTOs.Project;

// Create project request DTO
public class CreateProjectDto
{
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? ProjectManager { get; set; }
    public string? ContactNumber { get; set; }
    public string? ContractNumber { get; set; }
    public string Priority { get; set; } = string.Empty;
    public Guid ContractorId { get; set; }
}