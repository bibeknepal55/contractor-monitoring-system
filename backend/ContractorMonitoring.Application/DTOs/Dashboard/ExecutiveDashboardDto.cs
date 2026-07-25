namespace ContractorMonitoring.Application.DTOs.Dashboard;

// Executive dashboard summary DTO
public class ExecutiveDashboardDto
{
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int DelayedProjects { get; set; }
    public int TotalContractors { get; set; }
    public decimal TotalBudget { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal BudgetUtilization { get; set; }
    public decimal AverageProgress { get; set; }
    public int PendingApprovals { get; set; }
    public int ActivePerformanceBonds { get; set; }
    public int ExpiringGuarantees { get; set; }
    public List<ProjectStatusDistributionDto> ProjectStatusDistribution { get; set; } = new();
    public List<MonthlyProgressDto> MonthlyProgress { get; set; } = new();
    public List<BudgetByProjectDto> TopProjectsByBudget { get; set; } = new();
    public List<DelaySummaryDto> RecentDelays { get; set; } = new();
}

public class ProjectStatusDistributionDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class MonthlyProgressDto
{
    public string Month { get; set; } = string.Empty;
    public decimal PlannedProgress { get; set; }
    public decimal ActualProgress { get; set; }
}

public class BudgetByProjectDto
{
    public string ProjectName { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public decimal Spent { get; set; }
    public int Progress { get; set; }
}

public class DelaySummaryDto
{
    public string ProjectName { get; set; } = string.Empty;
    public string ContractorName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int Days { get; set; }
    public string DelayCategory { get; set; } = string.Empty;
    public string ImpactLevel { get; set; } = string.Empty;
}