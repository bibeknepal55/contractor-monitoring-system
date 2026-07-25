using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Domain.Constants;
using ContractorMonitoring.Domain.Entities;
using ContractorMonitoring.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/gdpr")]
[ApiController]
[Authorize]
public class GdprController : ControllerBase
{
    private readonly IGdprService _gdpr;
    private readonly ApplicationDbContext _db;

    public GdprController(IGdprService gdpr, ApplicationDbContext db) { _gdpr = gdpr; _db = db; }

    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

    // GET /api/v1/gdpr/export — export own data
    [HttpGet("export")]
    public async Task<IActionResult> ExportMyData()
    {
        var json = await _gdpr.ExportUserDataAsync(UserId);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"my-data-{DateTime.UtcNow:yyyyMMdd}.json");
    }

    // POST /api/v1/gdpr/erasure-request — request data erasure
    [HttpPost("erasure-request")]
    public async Task<ActionResult<ApiResponse<bool>>> RequestErasure()
    {
        var existing = await _db.GdprRequests.AnyAsync(r =>
            r.SubjectUserId == UserId && r.Status == "Pending" && !r.IsDeleted);
        if (existing)
            return BadRequest(ApiResponse<bool>.Fail("An erasure request is already pending"));

        _db.GdprRequests.Add(new GdprRequest
        {
            Id = Guid.NewGuid(), SubjectUserId = UserId,
            RequestType = "Erasure", Status = "Pending",
            RequestedBy = User.FindFirst(ClaimTypes.Email)?.Value,
            CreatedAt = DateTime.UtcNow, CreatedBy = UserId.ToString(), TenantId = Guid.Empty
        });
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Erasure request submitted. Will be processed within 30 days."));
    }

    // GET /api/v1/gdpr/requests — SuperAdmin: list all requests
    [HttpGet("requests")]
    [Authorize(Policy = Permissions.UserManagement.View)]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetRequests()
    {
        var requests = await _db.GdprRequests
            .Select(r => new { r.Id, r.SubjectUserId, r.RequestType, r.Status, r.RequestedBy, r.CreatedAt, r.ProcessedAt })
            .ToListAsync();
        return Ok(ApiResponse<List<object>>.Ok(requests.Cast<object>().ToList()));
    }

    // POST /api/v1/gdpr/requests/{id}/process — SuperAdmin: process erasure
    [HttpPost("requests/{id:guid}/process")]
    [Authorize(Policy = Permissions.UserManagement.Delete)]
    public async Task<ActionResult<ApiResponse<bool>>> ProcessErasure(Guid id)
    {
        var request = await _db.GdprRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (request == null) return NotFound(ApiResponse<bool>.Fail("Request not found"));
        if (request.Status != "Pending") return BadRequest(ApiResponse<bool>.Fail("Request already processed"));

        await _gdpr.EraseUserDataAsync(request.SubjectUserId, UserId.ToString());

        request.Status = "Completed";
        request.ProcessedAt = DateTime.UtcNow;
        request.ProcessedBy = UserId.ToString();
        request.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "User data erased successfully"));
    }
}

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/audit-trail")]
[ApiController]
[Authorize(Policy = Permissions.UserManagement.View)]
public class AuditTrailController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditTrailService _audit;

    public AuditTrailController(ApplicationDbContext db, IAuditTrailService audit) { _db = db; _audit = audit; }

    // GET /api/v1/audit-trail
    [HttpGet]
    public async Task<ActionResult<PagedResponse<object>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        [FromQuery] string? entityName = null, [FromQuery] string? action = null)
    {
        var query = _db.AuditTrails.AsQueryable();
        if (!string.IsNullOrEmpty(entityName)) query = query.Where(a => a.EntityName == entityName);
        if (!string.IsNullOrEmpty(action)) query = query.Where(a => a.Action == action);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new { a.Id, a.EntityName, a.EntityId, a.Action, a.UserEmail, a.IpAddress, a.ChangedColumns, a.CreatedAt })
            .ToListAsync();

        return Ok(new PagedResponse<object>
        {
            Data = items.Cast<object>().ToList(),
            Page = page, PageSize = pageSize, TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }

    // GET /api/v1/audit-trail/verify — verify hash chain integrity
    [HttpGet("verify")]
    public async Task<ActionResult<ApiResponse<bool>>> VerifyIntegrity()
    {
        var isValid = await _audit.VerifyChainIntegrityAsync(Guid.Empty);
        return Ok(ApiResponse<bool>.Ok(isValid, isValid ? "Chain integrity verified" : "INTEGRITY FAILURE DETECTED"));
    }
}
