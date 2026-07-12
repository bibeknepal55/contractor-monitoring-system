using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Physical progress tracking entity
public class PhysicalProgress : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public DateTime ProgressDate { get; set; }
    public decimal PlannedProgress { get; set; } // Percentage
    public decimal ActualProgress { get; set; } // Percentage
    public decimal Variance => ActualProgress - PlannedProgress;
    public string? ActivityDescription { get; set; }
    public string? Bottlenecks { get; set; }
    public string? MitigationPlan { get; set; }
    public string? SupportingDocument { get; set; } // File path
    public string? ReportedBy { get; set; }
    public string? VerifiedBy { get; set; }
    public string Status { get; set; } = string.Empty; // OnTrack, Delayed, Ahead, Critical

    // Navigation properties
    public Project Project { get; set; } = null!;
}