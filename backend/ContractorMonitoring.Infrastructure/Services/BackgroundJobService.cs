using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ContractorMonitoring.Infrastructure.Data;

namespace ContractorMonitoring.Infrastructure.Services;

public class BackgroundJobService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundJobService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6);

    public BackgroundJobService(IServiceScopeFactory scopeFactory, ILogger<BackgroundJobService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Job Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                await CheckExpiringPerformanceBonds(db, emailService, stoppingToken);
                await CheckExpiringAdvancePaymentGuarantees(db, emailService, stoppingToken);
                await CheckLicenseExpiry(db, emailService, stoppingToken);
                await CheckDelayedProjects(db, emailService, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background job execution");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckExpiringPerformanceBonds(
        ApplicationDbContext db, IEmailService emailService, CancellationToken ct)
    {
        var threshold = DateTime.UtcNow.AddDays(30);
        var expiring = await db.PerformanceBonds
            .Where(b => b.Status == "Active" && b.ExpiryDate <= threshold && b.ExpiryDate > DateTime.UtcNow)
            .Select(b => new { b.BondNumber, b.ExpiryDate, b.ProjectId })
            .ToListAsync(ct);

        foreach (var bond in expiring)
        {
            var days = (bond.ExpiryDate - DateTime.UtcNow).Days;
            _logger.LogWarning("Performance Bond {BondNumber} expires in {Days} days on {ExpiryDate}",
                bond.BondNumber, days, bond.ExpiryDate.ToShortDateString());

            await emailService.SendAsync(
                subject: $"Performance Bond Expiring: {bond.BondNumber}",
                body: $"Performance Bond <strong>{bond.BondNumber}</strong> will expire in <strong>{days} days</strong> on {bond.ExpiryDate:dd MMM yyyy}. Please arrange renewal.",
                eventType: "BondExpiry");
        }
    }

    private async Task CheckExpiringAdvancePaymentGuarantees(
        ApplicationDbContext db, IEmailService emailService, CancellationToken ct)
    {
        var threshold = DateTime.UtcNow.AddDays(30);
        var expiring = await db.AdvancePaymentGuarantees
            .Where(g => g.Status == "Active" && g.ExpiryDate <= threshold && g.ExpiryDate > DateTime.UtcNow)
            .Select(g => new { g.GuaranteeNumber, g.ExpiryDate })
            .ToListAsync(ct);

        foreach (var apg in expiring)
        {
            var days = (apg.ExpiryDate - DateTime.UtcNow).Days;
            _logger.LogWarning("APG {GuaranteeNumber} expires in {Days} days on {ExpiryDate}",
                apg.GuaranteeNumber, days, apg.ExpiryDate.ToShortDateString());

            await emailService.SendAsync(
                subject: $"Advance Payment Guarantee Expiring: {apg.GuaranteeNumber}",
                body: $"APG <strong>{apg.GuaranteeNumber}</strong> will expire in <strong>{days} days</strong> on {apg.ExpiryDate:dd MMM yyyy}.",
                eventType: "GuaranteeExpiry");
        }
    }

    private async Task CheckLicenseExpiry(
        ApplicationDbContext db, IEmailService emailService, CancellationToken ct)
    {
        var threshold = DateTime.UtcNow.AddDays(30);
        var expiring = await db.ContractorOfficeDetails
            .Where(c => c.Status == "Active" &&
                        c.LicenseExpiryDate.HasValue &&
                        c.LicenseExpiryDate.Value <= threshold)
            .Select(c => new { c.CompanyName, c.LicenseExpiryDate })
            .ToListAsync(ct);

        foreach (var contractor in expiring)
        {
            _logger.LogWarning("Contractor {CompanyName} license expires on {ExpiryDate}",
                contractor.CompanyName, contractor.LicenseExpiryDate?.ToShortDateString());

            await emailService.SendAsync(
                subject: $"Contractor License Expiring: {contractor.CompanyName}",
                body: $"Contractor <strong>{contractor.CompanyName}</strong> license expires on {contractor.LicenseExpiryDate:dd MMM yyyy}. Please ensure renewal.",
                eventType: "LicenseExpiry");
        }
    }

    private async Task CheckDelayedProjects(
        ApplicationDbContext db, IEmailService emailService, CancellationToken ct)
    {
        var delayed = await db.Projects
            .Where(p => p.Status == "InProgress" && p.EndDate.HasValue && p.EndDate.Value < DateTime.UtcNow)
            .Select(p => new { p.ProjectName, p.EndDate })
            .ToListAsync(ct);

        foreach (var project in delayed)
        {
            var delayDays = (DateTime.UtcNow - project.EndDate!.Value).Days;
            _logger.LogWarning("Project {ProjectName} is delayed by {Days} days", project.ProjectName, delayDays);

            await emailService.SendAsync(
                subject: $"Project Delayed: {project.ProjectName}",
                body: $"Project <strong>{project.ProjectName}</strong> is delayed by <strong>{delayDays} days</strong> past its end date.",
                eventType: "ProjectDelay");
        }
    }
}
