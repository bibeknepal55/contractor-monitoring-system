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

    public async Task<ApiResponse<ExecutiveDashboardDto>> Handle(GetExecutiveDashboardQuery request, CancellationToken cancellationToken)
    {
        var projects = await _unitOfWork.Projects.GetAllAsync();
        var tenantProjects = projects.Where(p => p.TenantId == request.TenantId).ToList();
        var contractors = await _unitOfWork.ContractorOfficeDetails.GetAllAsync();
        var tenantContractors = contractors.Where(c => c.TenantId == request.TenantId).ToList();
        var progressList = await _unitOfWork.PhysicalProgresses.GetAllAsync();
        var bonds = await _unitOfWork.PerformanceBonds.GetAllAsync();
        var guarantees = await _unitOfWork.AdvancePaymentGuarantees.GetAllAsync();
        var delays = await _unitOfWork.DelayReasons.GetAllAsync();
        var timeExtensions = await _unitOfWork.TimeExtensions.GetAllAsync();

        var dashboard = new ExecutiveDashboardDto
        {
            TotalProjects = tenantProjects.Count,
            ActiveProjects = tenantProjects.Count(p => p.Status == "InProgress"),
            CompletedProjects = tenantProjects.Count(p => p.Status == "Completed"),
            DelayedProjects = tenantProjects.Count(p => p.Status == "OnHold"),
            TotalContractors = tenantContractors.Count,
            TotalBudget = tenantProjects.Sum(p => p.Budget),
            TotalSpent = tenantProjects.Sum(p => p.ActualCost ?? 0),
            BudgetUtilization = tenantProjects.Sum(p => p.Budget) > 0
                ? (tenantProjects.Sum(p => p.ActualCost ?? 0) / tenantProjects.Sum(p => p.Budget)) * 100
                : 0,
            AverageProgress = tenantProjects.Any() ? Convert.ToDecimal(tenantProjects.Average(p => p.ProgressPercentage ?? 0)) : 0,
            PendingApprovals = timeExtensions.Count(t => t.Status == "Pending" && tenantProjects.Any(p => p.Id == t.ProjectId)),
            ActivePerformanceBonds = bonds.Count(b => b.Status == "Active" && tenantProjects.Any(p => p.Id == b.ProjectId)),
            ExpiringGuarantees = guarantees.Count(g => g.ExpiryDate <= DateTime.UtcNow.AddDays(30) && g.Status == "Active"),

            ProjectStatusDistribution = tenantProjects
                .GroupBy(p => p.Status)
                .Select(g => new ProjectStatusDistributionDto { Status = g.Key, Count = g.Count() })
                .ToList(),

            TopProjectsByBudget = tenantProjects
                .OrderByDescending(p => p.Budget)
                .Take(10)
                .Select(p => new BudgetByProjectDto
                {
                    ProjectName = p.ProjectName,
                    Budget = p.Budget,
                    Spent = p.ActualCost ?? 0
                })
                .ToList(),

            RecentDelays = delays
                .Where(d => tenantProjects.Any(p => p.Id == d.ProjectId))
                .OrderByDescending(d => d.DelayDays)
                .Take(5)
                .Select(d => new DelaySummaryDto
                {
                    ProjectName = tenantProjects.FirstOrDefault(p => p.Id == d.ProjectId)?.ProjectName ?? "Unknown",
                    DelayCategory = d.DelayCategory,
                    DelayDays = d.DelayDays,
                    ImpactLevel = d.ImpactLevel
                })
                .ToList()
        };

        return ApiResponse<ExecutiveDashboardDto>.Ok(dashboard, "Dashboard data retrieved successfully");
    }
}