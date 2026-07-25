using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Domain.Constants;
using ContractorMonitoring.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ContractorMonitoring.API.Controllers.V1;

// Phase 2: Tenant management
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tenants")]
[ApiController]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly ITenantManagementService _tenantSvc;
    private readonly ApplicationDbContext _db;

    public TenantsController(ITenantManagementService tenantSvc, ApplicationDbContext db)
    { _tenantSvc = tenantSvc; _db = db; }

    [HttpGet]
    [Authorize(Policy = Permissions.UserManagement.View)]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetAll()
    {
        var tenants = await _db.Tenants
            .Select(t => new { t.Id, t.Name, t.Subdomain, t.IsActive, t.Plan, t.AdminEmail, t.PrimaryColor, t.LogoUrl, t.CreatedAt })
            .ToListAsync();
        return Ok(ApiResponse<List<object>>.Ok(tenants.Cast<object>().ToList()));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.UserManagement.Create)]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CreateTenantRequest req)
    {
        try
        {
            var tenant = await _tenantSvc.CreateTenantAsync(req.Name, req.Subdomain, req.AdminEmail);
            return Ok(ApiResponse<object>.Ok(new { tenant.Id, tenant.Name, tenant.Subdomain }, "Tenant created"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:guid}/branding")]
    [Authorize(Policy = Permissions.UserManagement.Update)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateBranding(Guid id, [FromBody] TenantBrandingRequest req)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == id);
        if (tenant == null) return NotFound(ApiResponse<bool>.Fail("Tenant not found"));
        tenant.PrimaryColor = req.PrimaryColor;
        tenant.SecondaryColor = req.SecondaryColor;
        tenant.LogoUrl = req.LogoUrl;
        tenant.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Branding updated"));
    }

    [HttpPut("{id:guid}/security")]
    [Authorize(Policy = Permissions.UserManagement.Update)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateSecurity(Guid id, [FromBody] TenantSecurityRequest req)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == id);
        if (tenant == null) return NotFound(ApiResponse<bool>.Fail("Tenant not found"));
        tenant.IpAllowlist = req.IpAllowlist;
        tenant.GeoBlockEnabled = req.GeoBlockEnabled;
        tenant.AllowedCountries = req.AllowedCountries;
        tenant.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Security policy updated"));
    }
}

public record CreateTenantRequest(string Name, string Subdomain, string AdminEmail);
public record TenantBrandingRequest(string? PrimaryColor, string? SecondaryColor, string? LogoUrl);
public record TenantSecurityRequest(string? IpAllowlist, bool GeoBlockEnabled, string? AllowedCountries);

// Phase 4: Business Intelligence
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/bi")]
[ApiController]
[Authorize(Policy = Permissions.Dashboard.View)]
public class BiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IPerformanceScoringService _scoring;
    private readonly IPredictiveAlertService _alerts;

    public BiController(ApplicationDbContext db, IPerformanceScoringService scoring, IPredictiveAlertService alerts)
    { _db = db; _scoring = scoring; _alerts = alerts; }

    [HttpGet("contractor-scores")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetScores()
    {
        var scores = await _db.ContractorPerformanceScores
            .Join(_db.ContractorOfficeDetails, s => s.ContractorId, c => c.Id,
                (s, c) => new { c.CompanyName, s.OverallScore, s.Grade, s.DelayScore, s.LabTestScore, s.BondComplianceScore, s.ProgressScore, s.ComputedAt })
            .OrderByDescending(s => s.OverallScore)
            .ToListAsync();
        return Ok(ApiResponse<List<object>>.Ok(scores.Cast<object>().ToList()));
    }

    [HttpGet("predictive-alerts")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetAlerts()
    {
        var alerts = await _db.PredictiveAlerts
            .Where(a => !a.IsAcknowledged)
            .Join(_db.Projects, a => a.ProjectId, p => p.Id,
                (a, p) => new { a.Id, p.ProjectName, a.AlertType, a.Severity, a.Message, a.ConfidenceScore, a.CreatedAt })
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
        return Ok(ApiResponse<List<object>>.Ok(alerts.Cast<object>().ToList()));
    }

    [HttpPost("predictive-alerts/{id:guid}/acknowledge")]
    public async Task<ActionResult<ApiResponse<bool>>> AcknowledgeAlert(Guid id)
    {
        var alert = await _db.PredictiveAlerts.FirstOrDefaultAsync(a => a.Id == id);
        if (alert == null) return NotFound(ApiResponse<bool>.Fail("Alert not found"));
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        alert.IsAcknowledged = true;
        alert.AcknowledgedBy = userId;
        alert.AcknowledgedAt = DateTime.UtcNow;
        alert.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Alert acknowledged"));
    }

    [HttpPost("compute-scores")]
    [Authorize(Policy = Permissions.UserManagement.View)]
    public async Task<ActionResult<ApiResponse<bool>>> TriggerScoring()
    {
        await _scoring.ComputeAllScoresAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Scores computed"));
    }
}

