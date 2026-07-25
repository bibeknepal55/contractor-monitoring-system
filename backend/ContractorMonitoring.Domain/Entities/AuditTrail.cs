using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Phase 1: Tamper-proof audit trail — each entry chains to the previous via SHA-256
public class AuditTrail : AuditableEntity
{
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;       // Created, Updated, Deleted, Viewed
    public string? OldValues { get; set; }                   // JSON snapshot before change
    public string? NewValues { get; set; }                   // JSON snapshot after change
    public string? ChangedColumns { get; set; }              // CSV of changed property names
    public Guid? UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string PreviousHash { get; set; } = string.Empty; // SHA-256 of previous row
    public string CurrentHash { get; set; } = string.Empty;  // SHA-256 of this row's content
    public User? User { get; set; }
}

// Phase 1: GDPR — data erasure requests
public class GdprRequest : AuditableEntity
{
    public Guid SubjectUserId { get; set; }
    public string RequestType { get; set; } = string.Empty;  // "Export", "Erasure"
    public string Status { get; set; } = "Pending";          // Pending, Processing, Completed, Rejected
    public string? RequestedBy { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }
    public string? Notes { get; set; }
    public string? ExportFilePath { get; set; }
    public User? SubjectUser { get; set; }
}
