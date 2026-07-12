using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Time extension management entity
public class TimeExtension : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public string ExtensionNumber { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public int DaysRequested { get; set; }
    public int? DaysGranted { get; set; }
    public DateTime OriginalCompletionDate { get; set; }
    public DateTime? RevisedCompletionDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? SupportingDocument { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Approved, Rejected
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? Remarks { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
}