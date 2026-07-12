using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Approval workflow entity for tracking approvals across modules
public class ApprovalWorkflow : AuditableEntity
{
    public string ModuleName { get; set; } = string.Empty; // Project, TimeExtension, PriceAdjustment, etc.
    public Guid RecordId { get; set; } // ID of the record being approved
    public string RecordTitle { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Submitted, Approved, Rejected, Returned
    public string Comments { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public int ApprovalLevel { get; set; } = 1; // Multi-level approval support
    public string Status { get; set; } = string.Empty; // Pending, Approved, Rejected
    public string? PreviousStatus { get; set; }
    public string? NextApprover { get; set; }
}