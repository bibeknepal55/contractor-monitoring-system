namespace ContractorMonitoring.Application.DTOs.ApprovalWorkflow;

public class ApprovalWorkflowDto
{
    public Guid Id { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public Guid RecordId { get; set; }
    public string RecordTitle { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public int ApprovalLevel { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateApprovalRequestDto
{
    public string ModuleName { get; set; } = string.Empty;
    public Guid RecordId { get; set; }
    public string RecordTitle { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
    public int ApprovalLevel { get; set; } = 1;
}

public class ProcessApprovalDto
{
    public string Action { get; set; } = string.Empty; // Approved or Rejected
    public string Comments { get; set; } = string.Empty;
}

public class UpdateApprovalRequestDto
{
    public string ModuleName { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string RecordTitle { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
    public int ApprovalLevel { get; set; } = 1;
}

