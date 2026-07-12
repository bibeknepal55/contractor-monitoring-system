namespace ContractorMonitoring.Application.DTOs.Project;

// Project response DTO
public class ProjectDto
{
    public Guid Id { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public decimal? ActualCost { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? ProjectManager { get; set; }
    public string? ContactNumber { get; set; }
    public string? ContractNumber { get; set; }
    public string Priority { get; set; } = string.Empty;
    public double? ProgressPercentage { get; set; }
    public Guid ContractorId { get; set; }
    public string ContractorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}