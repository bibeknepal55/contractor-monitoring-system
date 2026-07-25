using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Dashboard;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Dashboard.Queries.GetExecutiveDashboard;

public class GetExecutiveDashboardQueryHandler : IRequestHandler<GetExecutiveDashboardQuery, ApiResponse<ExecutiveDashboardDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetExecutiveDashboardQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<ExecutiveDashboardDto>> Handle(
        GetExecutiveDashboardQuery request, CancellationToken cancellationToken)
    {
        var tid = request.TenantId;
        var now = DateTime.UtcNow;

        // Filtered queries — only load what belongs to this tenant
        var projects    = (await _unitOfWork.Projects.GetAllAsync()).Where(p => p.TenantId == tid).ToList();
        var contractors = (await _unitOfWork.ContractorOfficeDetails.GetAllAsync()).Where(c => c.TenantId == tid).ToList();
        var bonds       = (await _unitOfWork.PerformanceBonds.GetAllAsync()).ToList();
        var guarantees  = (await _unitOfWork.AdvancePaymentGuarantees.GetAllAsync()).ToList();
        var delays      = (await _unitOfWork.DelayReasons.GetAllAsync()).ToList();
        var extensions  = (await _unitOfWork.TimeExtensions.GetAllAsync()).ToList();

        var projectIds  = projects.Select(p => p.Id).ToHashSet();
        var totalBudget = projects.Sum(p => p.Budget);
        var totalSpent  = projects.Sum(p => p.ActualCost ?? 0);

        var recentDelays = delays
            .Where(d => projectIds.Contains(d.ProjectId))
            .OrderByDescending(d => d.DelayDays)
            .Take(5)
            .Select(d =>
            {
                var proj = projects.FirstOrDefault(p => p.Id == d.ProjectId);
                var contractor = proj != null
                    ? contractors.FirstOrDefault(c => c.Id == proj.ContractorId)
                    : null;
                return new DelaySummaryDto
                {
                    ProjectName    = proj?.ProjectName ?? "Unknown",
                    ContractorName = contractor?.CompanyName ?? string.Empty,
                    Reason         = d.Description,
                    Days           = d.DelayDays,
                    DelayCategory  = d.DelayCategory,
                    ImpactLevel    = d.ImpactLevel
                };
            })
            .ToList();

        var dashboard = new ExecutiveDashboardDto
        {
            TotalProjects     = projects.Count,
            ActiveProjects    = projects.Count(p => p.Status == "InProgress"),
            CompletedProjects = projects.Count(p => p.Status == "Completed"),
            // Fix: count truly delayed projects (status Delayed OR past end date)
            DelayedProjects   = projects.Count(p =>
                p.Status == "Delayed" ||
                (p.Status == "InProgress" && p.EndDate.HasValue && p.EndDate.Value < now)),
            TotalContractors       = contractors.Count,
            TotalBudget            = totalBudget,
            TotalSpent             = totalSpent,
            BudgetUtilization      = totalBudget > 0 ? Math.Round(totalSpent / totalBudget * 100, 2) : 0,
            AverageProgress        = projects.Any()
                ? Convert.ToDecimal(projects.Average(p => p.ProgressPercentage ?? 0))
                : 0,
            PendingApprovals       = extensions.Count(t => t.Status == "Pending" && projectIds.Contains(t.ProjectId)),
            ActivePerformanceBonds = bonds.Count(b => b.Status == "Active" && projectIds.Contains(b.ProjectId)),
            ExpiringGuarantees     = guarantees.Count(g =>
                g.Status == "Active" && g.ExpiryDate <= now.AddDays(30) && projectIds.Contains(g.ProjectId)),

            ProjectStatusDistribution = projects
                .GroupBy(p => p.Status)
                .Select(g => new ProjectStatusDistributionDto { Status = g.Key, Count = g.Count() })
                .ToList(),

            TopProjectsByBudget = projects
                .OrderByDescending(p => p.Budget)
                .Take(10)
                .Select(p => new BudgetByProjectDto
                {
                    ProjectName = p.ProjectName,
                    Budget      = p.Budget,
                    Spent       = p.ActualCost ?? 0,
                    Progress    = (int)(p.ProgressPercentage ?? 0)
                })
                .ToList(),

            RecentDelays = recentDelays
        };

        return ApiResponse<ExecutiveDashboardDto>.Ok(dashboard, "Dashboard data retrieved successfully");
    }
}
