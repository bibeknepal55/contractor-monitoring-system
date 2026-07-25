using Hangfire;
using Microsoft.Extensions.Logging;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ContractorMonitoring.Infrastructure.Services;

// Phase 6: All background jobs registered with Hangfire
public class HangfireJobService
{
    private readonly ILogger<HangfireJobService> _logger;

    public HangfireJobService(ILogger<HangfireJobService> logger) => _logger = logger;

    public static void RegisterRecurringJobs()
    {
        // Phase 4: BI scoring — daily at 2am
        RecurringJob.AddOrUpdate<IPerformanceScoringService>(
            "contractor-scoring", s => s.ComputeAllScoresAsync(), "0 2 * * *");

        // Phase 4: Predictive alerts — every 6 hours
        RecurringJob.AddOrUpdate<IPredictiveAlertService>(
            "predictive-alerts", s => s.EvaluateProjectsAsync(), "0 */6 * * *");

        // Phase 1: Expiry notifications — daily at 8am
        RecurringJob.AddOrUpdate<ExpiryNotificationJob>(
            "expiry-notifications", j => j.RunAsync(), "0 8 * * *");

        // Phase 1: Audit chain verification — weekly Sunday 3am
        RecurringJob.AddOrUpdate<AuditChainVerificationJob>(
            "audit-chain-verify", j => j.RunAsync(), "0 3 * * 0");

        // Phase 6: Clean up expired revoked tokens — daily midnight
        RecurringJob.AddOrUpdate<TokenCleanupJob>(
            "token-cleanup", j => j.RunAsync(), "0 0 * * *");
    }
}

// Expiry notification job (replaces BackgroundJobService)
public class ExpiryNotificationJob
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _email;
    private readonly ILogger<ExpiryNotificationJob> _logger;

    public ExpiryNotificationJob(ApplicationDbContext db, IEmailService email, ILogger<ExpiryNotificationJob> logger)
    {
        _db = db; _email = email; _logger = logger;
    }

    public async Task RunAsync()
    {
        var threshold = DateTime.UtcNow.AddDays(30);

        var expiringBonds = await _db.PerformanceBonds
            .Where(b => b.Status == "Active" && b.ExpiryDate <= threshold && b.ExpiryDate > DateTime.UtcNow)
            .ToListAsync();

        foreach (var bond in expiringBonds)
        {
            var days = (bond.ExpiryDate - DateTime.UtcNow).Days;
            _logger.LogWarning("Performance Bond {BondNumber} expires in {Days} days", bond.BondNumber, days);
            await _email.SendAsync(
                $"Performance Bond Expiring: {bond.BondNumber}",
                $"Bond <strong>{bond.BondNumber}</strong> expires in <strong>{days} days</strong> on {bond.ExpiryDate:dd MMM yyyy}.",
                "BondExpiry");
        }

        var expiringApgs = await _db.AdvancePaymentGuarantees
            .Where(g => g.Status == "Active" && g.ExpiryDate <= threshold && g.ExpiryDate > DateTime.UtcNow)
            .ToListAsync();

        foreach (var apg in expiringApgs)
        {
            var days = (apg.ExpiryDate - DateTime.UtcNow).Days;
            await _email.SendAsync(
                $"APG Expiring: {apg.GuaranteeNumber}",
                $"APG <strong>{apg.GuaranteeNumber}</strong> expires in <strong>{days} days</strong>.",
                "GuaranteeExpiry");
        }

        var expiringLicenses = await _db.ContractorOfficeDetails
            .Where(c => c.Status == "Active" && c.LicenseExpiryDate.HasValue && c.LicenseExpiryDate.Value <= threshold)
            .ToListAsync();

        foreach (var c in expiringLicenses)
        {
            _logger.LogWarning("Contractor {Name} license expires on {Date}", c.CompanyName, c.LicenseExpiryDate);
            await _email.SendAsync(
                $"License Expiring: {c.CompanyName}",
                $"Contractor <strong>{c.CompanyName}</strong> license expires on {c.LicenseExpiryDate:dd MMM yyyy}.",
                "LicenseExpiry");
        }
    }
}

public class AuditChainVerificationJob
{
    private readonly IAuditTrailService _audit;
    private readonly ILogger<AuditChainVerificationJob> _logger;

    public AuditChainVerificationJob(IAuditTrailService audit, ILogger<AuditChainVerificationJob> logger)
    {
        _audit = audit; _logger = logger;
    }

    public async Task RunAsync()
    {
        var isValid = await _audit.VerifyChainIntegrityAsync(Guid.Empty);
        if (isValid)
            _logger.LogInformation("Audit chain integrity verified: PASS");
        else
            _logger.LogCritical("AUDIT CHAIN INTEGRITY FAILURE — possible tampering detected!");
    }
}

public class TokenCleanupJob
{
    private readonly ApplicationDbContext _db;

    public TokenCleanupJob(ApplicationDbContext db) => _db = db;

    public async Task RunAsync()
        => await _db.RevokedTokens
            .Where(t => t.ExpiresAt < DateTime.UtcNow)
            .ExecuteDeleteAsync();
}
