using Microsoft.EntityFrameworkCore;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Domain.Entities;
using ContractorMonitoring.Infrastructure.Data;

namespace ContractorMonitoring.Infrastructure.Services;

public class PerformanceScoringService : IPerformanceScoringService
{
    private readonly ApplicationDbContext _db;
    public PerformanceScoringService(ApplicationDbContext db) => _db = db;

    public async Task ComputeAllScoresAsync()
    {
        var contractors = await _db.ContractorOfficeDetails.ToListAsync();
        foreach (var c in contractors)
            await ComputeContractorScoreAsync(c.Id);
    }

    public async Task<decimal> ComputeContractorScoreAsync(Guid contractorId)
    {
        var projects = await _db.Projects.Where(p => p.ContractorId == contractorId).ToListAsync();
        if (!projects.Any()) return 0;

        var projectIds = projects.Select(p => p.Id).ToList();

        // Delay score: % of projects on time
        var delayedCount = projects.Count(p => p.EndDate.HasValue && p.EndDate < DateTime.UtcNow && p.Status == "InProgress");
        var delayScore = projects.Count > 0 ? (decimal)(projects.Count - delayedCount) / projects.Count * 100 : 100;

        // Lab test score: % passed
        var labTests = await _db.LabTests.Where(l => projectIds.Contains(l.ProjectId)).ToListAsync();
        var labScore = labTests.Any()
            ? (decimal)labTests.Count(l => string.Equals(l.TestResult, "Pass", StringComparison.OrdinalIgnoreCase)) / labTests.Count * 100
            : 100;

        // Bond compliance: active bonds not expired
        var bonds = await _db.PerformanceBonds.Where(b => projectIds.Contains(b.ProjectId)).ToListAsync();
        var bondScore = bonds.Any()
            ? (decimal)bonds.Count(b => b.Status == "Active" && b.ExpiryDate > DateTime.UtcNow) / bonds.Count * 100
            : 100;

        // Progress score: avg physical progress
        var progresses = await _db.PhysicalProgresses.Where(p => projectIds.Contains(p.ProjectId)).ToListAsync();
        var progressScore = progresses.Any()
            ? (decimal)progresses.Average(p => (double)(p.ActualProgress > 0 ? p.ActualProgress : 0m))
            : 50;

        var overall = delayScore * 0.3m + labScore * 0.25m + bondScore * 0.25m + progressScore * 0.2m;
        var grade = overall >= 90 ? "A" : overall >= 75 ? "B" : overall >= 60 ? "C" : overall >= 45 ? "D" : "F";

        var existing = await _db.ContractorPerformanceScores.FirstOrDefaultAsync(s => s.ContractorId == contractorId);
        if (existing != null)
        {
            existing.OverallScore = Math.Round(overall, 2);
            existing.DelayScore = Math.Round(delayScore, 2);
            existing.LabTestScore = Math.Round(labScore, 2);
            existing.BondComplianceScore = Math.Round(bondScore, 2);
            existing.ProgressScore = Math.Round(progressScore, 2);
            existing.Grade = grade;
            existing.ComputedAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _db.ContractorPerformanceScores.Add(new ContractorPerformanceScore
            {
                Id = Guid.NewGuid(), ContractorId = contractorId,
                OverallScore = Math.Round(overall, 2), DelayScore = Math.Round(delayScore, 2),
                LabTestScore = Math.Round(labScore, 2), BondComplianceScore = Math.Round(bondScore, 2),
                ProgressScore = Math.Round(progressScore, 2), Grade = grade,
                ComputedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow,
                CreatedBy = "System", TenantId = Guid.Empty
            });
        }
        await _db.SaveChangesAsync();
        return Math.Round(overall, 2);
    }
}

public class PredictiveAlertService : IPredictiveAlertService
{
    private readonly ApplicationDbContext _db;
    public PredictiveAlertService(ApplicationDbContext db) => _db = db;

    public async Task EvaluateProjectsAsync()
    {
        var projects = await _db.Projects.Where(p => p.Status == "InProgress" && p.EndDate.HasValue).ToListAsync();

        foreach (var project in projects)
        {
            var totalDays = (project.EndDate!.Value - project.StartDate).TotalDays;
            if (totalDays <= 0) continue;

            var elapsed = (DateTime.UtcNow - project.StartDate).TotalDays;
            var timelinePercent = elapsed / totalDays;

            var latestProgress = await _db.PhysicalProgresses
                .Where(p => p.ProjectId == project.Id)
                .OrderByDescending(p => p.ProgressDate)
                .Select(p => p.ActualProgress)
                .FirstOrDefaultAsync();

            // Rule: if >80% of timeline elapsed but <60% progress → delay risk
            if (timelinePercent >= 0.8 && latestProgress < 60)
            {
                var exists = await _db.PredictiveAlerts.AnyAsync(a =>
                    a.ProjectId == project.Id && a.AlertType == "DelayRisk" && !a.IsAcknowledged && !a.IsDeleted);

                if (!exists)
                {
                    var confidence = Math.Min(1.0m, (decimal)((timelinePercent - 0.8) * 5));
                    _db.PredictiveAlerts.Add(new PredictiveAlert
                    {
                        Id = Guid.NewGuid(), ProjectId = project.Id,
                        AlertType = "DelayRisk", Severity = confidence > 0.7m ? "High" : "Medium",
                        Message = $"Project '{project.ProjectName}' is {timelinePercent:P0} through timeline but only {latestProgress}% complete.",
                        ConfidenceScore = Math.Round(confidence, 2),
                        CreatedAt = DateTime.UtcNow, CreatedBy = "System", TenantId = project.TenantId
                    });
                }
            }
        }
        await _db.SaveChangesAsync();
    }
}
