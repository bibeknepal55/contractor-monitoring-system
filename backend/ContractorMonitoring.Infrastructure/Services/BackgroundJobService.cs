using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Infrastructure.Services;

// Background service for monitoring expiring bonds, guarantees, and sending notifications
public class BackgroundJobService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundJobService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6); // Run every 6 hours

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
                using (var scope = _scopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    await CheckExpiringPerformanceBonds(unitOfWork);
                    await CheckExpiringAdvancePaymentGuarantees(unitOfWork);
                    await CheckLicenseExpiry(unitOfWork);
                    await CheckDelayedProjects(unitOfWork);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background job execution");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckExpiringPerformanceBonds(IUnitOfWork unitOfWork)
    {
        var bonds = await unitOfWork.PerformanceBonds.GetAllAsync();
        var expiringBonds = bonds.Where(b =>
            b.Status == "Active" &&
            b.ExpiryDate <= DateTime.UtcNow.AddDays(30) &&
            b.ExpiryDate > DateTime.UtcNow);

        foreach (var bond in expiringBonds)
        {
            var daysUntilExpiry = (bond.ExpiryDate - DateTime.UtcNow).Days;
            _logger.LogWarning("Performance Bond {BondNumber} expires in {Days} days on {ExpiryDate}",
                bond.BondNumber, daysUntilExpiry, bond.ExpiryDate.ToShortDateString());

            // TODO: Send email notification
        }
    }

    private async Task CheckExpiringAdvancePaymentGuarantees(IUnitOfWork unitOfWork)
    {
        var guarantees = await unitOfWork.AdvancePaymentGuarantees.GetAllAsync();
        var expiringGuarantees = guarantees.Where(g =>
            g.Status == "Active" &&
            g.ExpiryDate <= DateTime.UtcNow.AddDays(30) &&
            g.ExpiryDate > DateTime.UtcNow);

        foreach (var guarantee in expiringGuarantees)
        {
            var daysUntilExpiry = (guarantee.ExpiryDate - DateTime.UtcNow).Days;
            _logger.LogWarning("APG {GuaranteeNumber} expires in {Days} days on {ExpiryDate}",
                guarantee.GuaranteeNumber, daysUntilExpiry, guarantee.ExpiryDate.ToShortDateString());

            // TODO: Send email notification
        }
    }

    private async Task CheckLicenseExpiry(IUnitOfWork unitOfWork)
    {
        var contractors = await unitOfWork.ContractorOfficeDetails.GetAllAsync();
        var expiredLicenses = contractors.Where(c =>
            c.LicenseExpiryDate.HasValue &&
            c.LicenseExpiryDate.Value <= DateTime.UtcNow.AddDays(30) &&
            c.Status == "Active");

        foreach (var contractor in expiredLicenses)
        {
            _logger.LogWarning("Contractor {CompanyName} license expires on {ExpiryDate}",
                contractor.CompanyName, contractor.LicenseExpiryDate?.ToShortDateString());
        }
    }

    private async Task CheckDelayedProjects(IUnitOfWork unitOfWork)
    {
        var projects = await unitOfWork.Projects.GetAllAsync();
        var delayedProjects = projects.Where(p =>
            p.Status == "InProgress" &&
            p.EndDate.HasValue &&
            p.EndDate.Value < DateTime.UtcNow);

        foreach (var project in delayedProjects)
        {
            var delayDays = (DateTime.UtcNow - project.EndDate!.Value).Days;
            _logger.LogWarning("Project {ProjectName} is delayed by {Days} days",
                project.ProjectName, delayDays);
        }
    }
}