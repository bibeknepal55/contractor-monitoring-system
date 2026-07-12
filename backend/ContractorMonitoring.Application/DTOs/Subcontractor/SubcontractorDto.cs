namespace ContractorMonitoring.Application.DTOs.Subcontractor;

public class SubcontractorDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string ScopeOfWork { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public decimal ContractAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PerformanceRating { get; set; }
    public string? LicenseNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSubcontractorDto
{
    public Guid ProjectId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string ScopeOfWork { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public decimal ContractAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? LicenseNumber { get; set; }
}

public class UpdateSubcontractorDto
{
    public string ScopeOfWork { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PerformanceRating { get; set; }
}