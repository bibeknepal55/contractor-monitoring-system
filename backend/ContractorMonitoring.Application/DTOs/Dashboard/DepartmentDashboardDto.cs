namespace ContractorMonitoring.Application.DTOs.Dashboard;

public class DepartmentDashboardDto
{
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int TotalUsers { get; set; }
    public decimal TotalBudget { get; set; }
    public int PendingApprovals { get; set; }
    public List<ProjectSummaryDto> RecentProjects { get; set; } = new();
    public List<ApprovalSummaryDto> PendingApprovalItems { get; set; } = new();
    public List<ActivitySummaryDto> RecentActivities { get; set; } = new();
}

public class ProjectSummaryDto
{
    public Guid Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public double ProgressPercentage { get; set; }
}

public class ApprovalSummaryDto
{
    public Guid Id { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string RecordTitle { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ActivitySummaryDto
{
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class PolicyPreviewRequestDto
{
    public Guid RoleId { get; set; }
    public List<Guid> PermissionIds { get; set; } = new();
}

public class PolicyPreviewResponseDto
{
    public string RoleName { get; set; } = string.Empty;
    public int AffectedUsers { get; set; }
    public List<ModuleDiffDto> ModuleChanges { get; set; } = new();
    public List<string> AddedPermissions { get; set; } = new();
    public List<string> RemovedPermissions { get; set; } = new();
}

public class ModuleDiffDto
{
    public string ModuleName { get; set; } = string.Empty;
    public List<string> Added { get; set; } = new();
    public List<string> Removed { get; set; } = new();
    public List<string> Unchanged { get; set; } = new();
}