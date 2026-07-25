using Microsoft.EntityFrameworkCore;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Domain.Entities;
using ContractorMonitoring.Infrastructure.Data;

namespace ContractorMonitoring.Infrastructure.Services;

public class TenantManagementService : ITenantManagementService
{
    private readonly ApplicationDbContext _context;

    public TenantManagementService(ApplicationDbContext context) => _context = context;

    public async Task<Tenant?> GetTenantBySubdomainAsync(string subdomain)
        => await _context.Tenants.FirstOrDefaultAsync(t => t.Subdomain == subdomain.ToLower() && t.IsActive);

    public async Task<Tenant?> GetTenantByIdAsync(Guid tenantId)
        => await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);

    public async Task<Tenant> CreateTenantAsync(string name, string subdomain, string adminEmail)
    {
        if (await _context.Tenants.AnyAsync(t => t.Subdomain == subdomain.ToLower()))
            throw new InvalidOperationException($"Subdomain '{subdomain}' is already taken");

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Subdomain = subdomain.ToLower().Trim(),
            AdminEmail = adminEmail,
            IsActive = true,
            Plan = "Standard",
            TrialEndsAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            TenantId = Guid.Empty
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();
        return tenant;
    }
}
