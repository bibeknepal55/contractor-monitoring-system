using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Reports;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Reports.Queries.GetReports;

public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, ApiResponse<object>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetReportsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<object>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var projects = await _unitOfWork.Projects.GetAllAsync();
        var tenantProjects = projects.Where(p => p.TenantId == request.TenantId).ToList();
        var contractors = await _unitOfWork.ContractorOfficeDetails.GetAllAsync();
        var tenantContractors = contractors.Where(c => c.TenantId == request.TenantId).ToList();
        var delays = await _unitOfWork.DelayReasons.GetAllAsync();
        var bonds = await _unitOfWork.PerformanceBonds.GetAllAsync();
        var guarantees = await _unitOfWork.AdvancePaymentGuarantees.GetAllAsync();
        var timeExtensions = await _unitOfWork.TimeExtensions.GetAllAsync();
        var progressList = await _unitOfWork.PhysicalProgresses.GetAllAsync();

        object reportData = request.Request.ReportType switch
        {
            "contractor-wise" => GenerateContractorWiseReport(tenantProjects, tenantContractors, request.Request),
            "project-wise" => GenerateProjectWiseReport(tenantProjects, tenantContractors, progressList, delays, timeExtensions, request.Request),
            "delay-analysis" => GenerateDelayAnalysisReport(tenantProjects, delays, request.Request),
            "pb-apg" => GeneratePBAPGReport(tenantProjects, bonds, guarantees, request.Request),
            "time-extension" => GenerateTimeExtensionReport(tenantProjects, timeExtensions, request.Request),
            "payment-pending" => GeneratePaymentPendingReport(tenantProjects, request.Request),
            _ => new { Message = "Invalid report type" }
        };

        return ApiResponse<object>.Ok(reportData, "Report generated successfully");
    }

    private object GenerateContractorWiseReport(List<Domain.Entities.Project> projects, List<Domain.Entities.ContractorOfficeDetail> contractors, ReportRequestDto filter)
    {
        return contractors.Select(c => new ContractorWiseReportDto
        {
            ContractorName = c.CompanyName,
            TotalProjects = projects.Count(p => p.ContractorId == c.Id),
            ActiveProjects = projects.Count(p => p.ContractorId == c.Id && p.Status == "InProgress"),
            CompletedProjects = projects.Count(p => p.ContractorId == c.Id && p.Status == "Completed"),
            TotalContractValue = projects.Where(p => p.ContractorId == c.Id).Sum(p => p.Budget),
            TotalPaymentsReceived = projects.Where(p => p.ContractorId == c.Id).Sum(p => p.ActualCost ?? 0),
            PendingPayments = projects.Where(p => p.ContractorId == c.Id).Sum(p => p.Budget - (p.ActualCost ?? 0)),
            Projects = projects.Where(p => p.ContractorId == c.Id).Select(p => new ProjectSummaryDto
            {
                ProjectName = p.ProjectName,
                Status = p.Status,
                Budget = p.Budget,
                Progress = (decimal)(p.ProgressPercentage ?? 0),
                StartDate = p.StartDate,
                EndDate = p.EndDate
            }).ToList()
        }).ToList();
    }

    private object GenerateProjectWiseReport(List<Domain.Entities.Project> projects, List<Domain.Entities.ContractorOfficeDetail> contractors, IEnumerable<Domain.Entities.PhysicalProgress> progressList, IEnumerable<Domain.Entities.DelayReason> delays, IEnumerable<Domain.Entities.TimeExtension> timeExtensions, ReportRequestDto filter)
    {
        return projects.Where(p => filter.ProjectId == null || p.Id == filter.ProjectId).Select(p => new ProjectWiseReportDto
        {
            ProjectName = p.ProjectName,
            ProjectCode = p.ProjectCode,
            ContractorName = contractors.FirstOrDefault(c => c.Id == p.ContractorId)?.CompanyName ?? "N/A",
            Status = p.Status,
            Budget = p.Budget,
            ActualCost = p.ActualCost ?? 0,
            ProgressPercentage = (decimal)(p.ProgressPercentage ?? 0),
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            TimeExtensions = timeExtensions.Count(t => t.ProjectId == p.Id),
            TotalDelays = delays.Count(d => d.ProjectId == p.Id),
            ProgressHistory = progressList.Where(pr => pr.ProjectId == p.Id).OrderBy(pr => pr.ProgressDate).Select(pr => new DTOs.PhysicalProgress.PhysicalProgressDto
            {
                ProgressDate = pr.ProgressDate,
                PlannedProgress = pr.PlannedProgress,
                ActualProgress = pr.ActualProgress,
                Status = pr.Status
            }).ToList(),
            Delays = delays.Where(d => d.ProjectId == p.Id).Select(d => new DelayReportDto
            {
                DelayCategory = d.DelayCategory,
                DelayDays = d.DelayDays,
                ImpactLevel = d.ImpactLevel
            }).ToList()
        }).ToList();
    }

    private object GenerateDelayAnalysisReport(List<Domain.Entities.Project> projects, IEnumerable<Domain.Entities.DelayReason> delays, ReportRequestDto filter)
    {
        return delays.Where(d => filter.StartDate == null || d.DelayStartDate >= filter.StartDate)
            .Where(d => filter.EndDate == null || d.DelayStartDate <= filter.EndDate)
            .Select(d => new DelayAnalysisReportDto
            {
                ProjectName = projects.FirstOrDefault(p => p.Id == d.ProjectId)?.ProjectName ?? "Unknown",
                DelayCategory = d.DelayCategory,
                Description = d.Description,
                DelayStartDate = d.DelayStartDate,
                DelayEndDate = d.DelayEndDate,
                DelayDays = d.DelayDays,
                ImpactLevel = d.ImpactLevel,
                ResponsibleParty = d.ResponsibleParty ?? "N/A",
                MitigationAction = d.MitigationAction ?? "N/A"
            }).ToList();
    }

    private object GeneratePBAPGReport(List<Domain.Entities.Project> projects, IEnumerable<Domain.Entities.PerformanceBond> bonds, IEnumerable<Domain.Entities.AdvancePaymentGuarantee> guarantees, ReportRequestDto filter)
    {
        var pbReports = bonds.Select(b => new PBAPGReportDto
        {
            ProjectName = projects.FirstOrDefault(p => p.Id == b.ProjectId)?.ProjectName ?? "Unknown",
            BondNumber = b.BondNumber,
            BondType = b.BondType,
            BondAmount = b.BondAmount,
            IssuingBank = b.IssuingBank,
            IssueDate = b.IssueDate,
            ExpiryDate = b.ExpiryDate,
            Status = b.Status,
            IsExpired = b.ExpiryDate < DateTime.UtcNow,
            IsExpiringSoon = b.ExpiryDate <= DateTime.UtcNow.AddDays(30) && b.ExpiryDate >= DateTime.UtcNow
        }).ToList();

        return pbReports;
    }

    private object GenerateTimeExtensionReport(List<Domain.Entities.Project> projects, IEnumerable<Domain.Entities.TimeExtension> timeExtensions, ReportRequestDto filter)
    {
        return timeExtensions.Select(t => new
        {
            ProjectName = projects.FirstOrDefault(p => p.Id == t.ProjectId)?.ProjectName ?? "Unknown",
            t.ExtensionNumber,
            t.RequestDate,
            t.DaysRequested,
            t.DaysGranted,
            t.OriginalCompletionDate,
            t.RevisedCompletionDate,
            t.Reason,
            t.Status,
            t.ApprovedBy,
            t.ApprovalDate
        }).ToList();
    }

    private object GeneratePaymentPendingReport(List<Domain.Entities.Project> projects, ReportRequestDto filter)
    {
        return projects.Select(p => new
        {
            p.ProjectName,
            p.ProjectCode,
            p.Budget,
            ActualCost = p.ActualCost ?? 0,
            PendingPayment = p.Budget - (p.ActualCost ?? 0),
            p.Status
        }).Where(p => p.PendingPayment > 0).OrderByDescending(p => p.PendingPayment).ToList();
    }
}