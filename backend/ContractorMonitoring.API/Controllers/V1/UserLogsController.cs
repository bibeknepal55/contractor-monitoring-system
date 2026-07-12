using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Domain.Constants;
using ContractorMonitoring.Infrastructure.Data;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/user-logs")]
[ApiController]
public class UserLogsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserLogsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/v1/user-logs
    [HttpGet]
    [Authorize(Policy = Permissions.UserManagement.View)]
    public async Task<ActionResult<PagedResponse<object>>> GetLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] string? activityType = null,
        [FromQuery] string? moduleName = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? ipAddress = null,
        [FromQuery] int? responseStatus = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] string? sortOrder = "desc")
    {
        var query = _context.UserActivityLogs.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(l =>
                l.UserName.Contains(search) ||
                l.UserEmail.Contains(search) ||
                l.Action.Contains(search) ||
                l.IpAddress.Contains(search));
        }

        if (!string.IsNullOrEmpty(activityType))
            query = query.Where(l => l.ActivityType == activityType);

        if (!string.IsNullOrEmpty(moduleName))
            query = query.Where(l => l.ModuleName == moduleName);

        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var uid))
            query = query.Where(l => l.UserId == uid);

        if (!string.IsNullOrEmpty(ipAddress))
            query = query.Where(l => l.IpAddress == ipAddress);

        if (responseStatus.HasValue)
            query = query.Where(l => l.ResponseStatus == responseStatus.Value);

        if (startDate.HasValue)
            query = query.Where(l => l.CreatedAt >= startDate.Value.ToUniversalTime());

        if (endDate.HasValue)
            query = query.Where(l => l.CreatedAt <= endDate.Value.ToUniversalTime());

        var allowedSortColumns = new HashSet<string> { "CreatedAt", "UserName", "ActivityType", "ModuleName", "IpAddress", "ResponseStatus" };
        var sortColumn = allowedSortColumns.Contains(sortBy ?? "CreatedAt") ? sortBy : "CreatedAt";
        var isAscending = string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase);

        query = isAscending
            ? query.OrderBy(l => EF.Property<object>(l, sortColumn!))
            : query.OrderByDescending(l => EF.Property<object>(l, sortColumn!));

        var totalCount = await query.CountAsync();

        var logs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new
            {
                l.Id,
                l.UserId,
                l.UserName,
                l.UserEmail,
                l.UserRole,
                l.ActivityType,
                l.ModuleName,
                l.Action,
                l.IpAddress,
                l.Location,
                l.DeviceInfo,
                l.RequestMethod,
                l.RequestUrl,
                l.ResponseStatus,
                l.SessionId,
                l.CreatedAt
            })
            .ToListAsync();

        return Ok(new PagedResponse<object>
        {
            Data = logs.Cast<object>().ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Message = "User logs retrieved successfully"
        });
    }

    // GET: api/v1/user-logs/{id}
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.UserManagement.View)]
    public async Task<ActionResult<ApiResponse<object>>> GetLogDetail(Guid id)
    {
        var log = await _context.UserActivityLogs.FindAsync(id);
        if (log == null)
            return NotFound(ApiResponse<object>.Fail("Log entry not found"));

        var sessionLogs = new List<object>();
        if (!string.IsNullOrEmpty(log.SessionId))
        {
            sessionLogs = await _context.UserActivityLogs
                .Where(l => l.SessionId == log.SessionId)
                .OrderBy(l => l.CreatedAt)
                .Select(l => new { l.Id, l.ActivityType, l.Action, l.RequestMethod, l.RequestUrl, l.ResponseStatus, l.CreatedAt })
                .Cast<object>().ToListAsync();
        }

        var userRecentLogs = await _context.UserActivityLogs
            .Where(l => l.UserId == log.UserId && l.Id != log.Id)
            .OrderByDescending(l => l.CreatedAt).Take(20)
            .Select(l => new { l.Id, l.ActivityType, l.Action, l.ModuleName, l.IpAddress, l.ResponseStatus, l.CreatedAt })
            .Cast<object>().ToListAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            Log = log,
            SessionContext = new { SessionId = log.SessionId, TotalRequests = sessionLogs.Count, Requests = sessionLogs },
            UserRecentActivity = userRecentLogs
        }, "Log detail retrieved"));
    }

    // GET: api/v1/user-logs/stats
    [HttpGet("stats")]
    [Authorize(Policy = Permissions.UserManagement.View)]
    public async Task<ActionResult<ApiResponse<object>>> GetStats()
    {
        var today = DateTime.UtcNow.Date;
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);

        var stats = new
        {
            TotalLoginsToday = await _context.UserActivityLogs.CountAsync(l => l.ActivityType == "Login" && l.CreatedAt >= today),
            TotalActivitiesToday = await _context.UserActivityLogs.CountAsync(l => l.CreatedAt >= today),
            FailedLoginAttempts = await _context.UserActivityLogs.CountAsync(l => l.ActivityType == "FailedLogin" && l.CreatedAt >= today),
            ErrorCount = await _context.UserActivityLogs.CountAsync(l => l.ActivityType == "Error" && l.CreatedAt >= today),
            ActiveUsersNow = await _context.UserActivityLogs.Where(l => l.CreatedAt >= oneHourAgo && l.UserId != null).Select(l => l.UserId).Distinct().CountAsync(),
            TopModules = await _context.UserActivityLogs.Where(l => l.CreatedAt >= today && l.ModuleName != "Auth" && l.ModuleName != "General").GroupBy(l => l.ModuleName).Select(g => new { Module = g.Key, Count = g.Count() }).OrderByDescending(x => x.Count).Take(10).ToListAsync(),
            ActivityBreakdown = await _context.UserActivityLogs.Where(l => l.CreatedAt >= today).GroupBy(l => l.ActivityType).Select(g => new { Type = g.Key, Count = g.Count() }).OrderByDescending(x => x.Count).ToListAsync()
        };

        return Ok(ApiResponse<object>.Ok(stats, "Stats retrieved"));
    }

    // GET: api/v1/user-logs/sessions/active
    [HttpGet("sessions/active")]
    [Authorize(Policy = Permissions.UserManagement.View)]
    public async Task<ActionResult<ApiResponse<object>>> GetActiveSessions()
    {
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var activeUsers = await _context.UserActivityLogs
            .Where(l => l.CreatedAt >= oneHourAgo && l.UserId != null && l.ActivityType != "Logout")
            .GroupBy(l => new { l.UserId, l.UserName, l.UserEmail, l.UserRole, l.IpAddress, l.DeviceInfo })
            .Select(g => new { g.Key.UserId, g.Key.UserName, g.Key.UserEmail, g.Key.UserRole, g.Key.IpAddress, g.Key.DeviceInfo, LastActivity = g.Max(l => l.CreatedAt), RequestCount = g.Count(), ActiveMinutes = (int)(DateTime.UtcNow - g.Min(l => l.CreatedAt)).TotalMinutes })
            .OrderByDescending(u => u.LastActivity).ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { TotalActiveUsers = activeUsers.Count, ActiveUsers = activeUsers }, "Active sessions retrieved"));
    }

    // GET: api/v1/user-logs/user/{userId}
    [HttpGet("user/{userId:guid}")]
    [Authorize(Policy = Permissions.UserManagement.View)]
    public async Task<ActionResult<PagedResponse<object>>> GetUserLogs(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var query = _context.UserActivityLogs.Where(l => l.UserId == userId).OrderByDescending(l => l.CreatedAt);
        var totalCount = await query.CountAsync();
        var logs = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(l => new { l.Id, l.ActivityType, l.ModuleName, l.Action, l.IpAddress, l.DeviceInfo, l.ResponseStatus, l.CreatedAt }).ToListAsync();

        return Ok(new PagedResponse<object> { Data = logs.Cast<object>().ToList(), Page = page, PageSize = pageSize, TotalCount = totalCount, TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize), Message = "User logs retrieved" });
    }

    // DELETE: api/v1/user-logs/purge
    [HttpDelete("purge")]
    [Authorize(Policy = Permissions.UserManagement.Delete)]
    public async Task<ActionResult<ApiResponse<object>>> PurgeOldLogs([FromQuery] int olderThanDays = 90)
    {
        int count;

        if (olderThanDays > 0)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
            // Use raw SQL to ensure hard delete
            count = await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM \"UserActivityLogs\" WHERE \"CreatedAt\" < {0}", cutoffDate);
        }
        else
        {
            // Delete ALL logs
            count = await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM \"UserActivityLogs\"");
        }

        return Ok(ApiResponse<object>.Ok(
            new { PurgedCount = count, OlderThanDays = olderThanDays },
            $"Successfully purged {count} logs"));
    }
}