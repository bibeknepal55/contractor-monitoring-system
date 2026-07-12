namespace ContractorMonitoring.Application.DTOs.TimeExtension;

public class TimeExtensionDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ExtensionNumber { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public int DaysRequested { get; set; }
    public int? DaysGranted { get; set; }
    public DateTime OriginalCompletionDate { get; set; }
    public DateTime? RevisedCompletionDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTimeExtensionDto
{
    public Guid ProjectId { get; set; }
    public DateTime RequestDate { get; set; }
    public int DaysRequested { get; set; }
    public DateTime OriginalCompletionDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}

public class UpdateTimeExtensionDto
{
    public int? DaysGranted { get; set; }
    public DateTime? RevisedCompletionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? Remarks { get; set; }
}