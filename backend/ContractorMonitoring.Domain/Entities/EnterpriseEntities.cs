using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Phase 4: Contractor performance score (computed by background job)
public class ContractorPerformanceScore : AuditableEntity
{
    public Guid ContractorId { get; set; }
    public decimal OverallScore { get; set; }          // 0-100
    public decimal DelayScore { get; set; }
    public decimal LabTestScore { get; set; }
    public decimal BondComplianceScore { get; set; }
    public decimal ProgressScore { get; set; }
    public string Grade { get; set; } = "C";           // A, B, C, D, F
    public DateTime ComputedAt { get; set; }
    public ContractorOfficeDetail? Contractor { get; set; }
}

// Phase 5: Document version control
public class ProjectDocument : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int Version { get; set; } = 1;
    public string? Description { get; set; }
    public Guid UploadedBy { get; set; }
    public bool IsLatest { get; set; } = true;
    public Guid? PreviousVersionId { get; set; }
    public Project? Project { get; set; }
}

// Phase 5: Comments with @mentions
public class RecordComment : AuditableEntity
{
    public string EntityName { get; set; } = string.Empty;   // "Project", "ApprovalWorkflow"
    public Guid EntityId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid AuthorId { get; set; }
    public Guid? ParentCommentId { get; set; }               // For threaded replies
    public string? MentionedUserIds { get; set; }            // JSON array of Guid strings
    public User? Author { get; set; }
}

// Phase 4: Predictive alert
public class PredictiveAlert : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public string AlertType { get; set; } = string.Empty;    // "DelayRisk", "BudgetOverrun", "BondExpiry"
    public string Severity { get; set; } = "Medium";         // Low, Medium, High, Critical
    public string Message { get; set; } = string.Empty;
    public decimal? ConfidenceScore { get; set; }            // 0-1 ML confidence
    public bool IsAcknowledged { get; set; } = false;
    public Guid? AcknowledgedBy { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public Project? Project { get; set; }
}
