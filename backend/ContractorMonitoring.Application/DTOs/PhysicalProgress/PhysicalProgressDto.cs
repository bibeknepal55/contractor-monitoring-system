namespace ContractorMonitoring.Application.DTOs.PhysicalProgress;

public class PhysicalProgressDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime ProgressDate { get; set; }
    public decimal PlannedProgress { get; set; }
    public decimal ActualProgress { get; set; }
    public decimal Variance { get; set; }
    public string? ActivityDescription { get; set; }
    public string? Bottlenecks { get; set; }
    public string? MitigationPlan { get; set; }
    public string? ReportedBy { get; set; }
    public string? VerifiedBy { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreatePhysicalProgressDto
{
    public Guid ProjectId { get; set; }
    public DateTime ProgressDate { get; set; }
    public decimal PlannedProgress { get; set; }
    public decimal ActualProgress { get; set; }
    public string? ActivityDescription { get; set; }
    public string? Bottlenecks { get; set; }
    public string? MitigationPlan { get; set; }
    public string? ReportedBy { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class UpdatePhysicalProgressDto
{
    public Guid ProjectId { get; set; }
    public DateTime ProgressDate { get; set; }
    public decimal PlannedProgress { get; set; }
    public decimal ActualProgress { get; set; }
    public string? ActivityDescription { get; set; }
    public string? Bottlenecks { get; set; }
    public string? MitigationPlan { get; set; }
    public string? VerifiedBy { get; set; }
    public string Status { get; set; } = string.Empty;
}