using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Session;
using ContractorMonitoring.Domain.Constants;
using ContractorMonitoring.Infrastructure.Data;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sessions")]
[ApiController]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SessionsController(ApplicationDbContext context) => _context = context;

    private Guid CurrentUserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
    private string CurrentJti => User.FindFirst("jti")?.Value ?? string.Empty;

    // GET /api/v1/sessions - List all active sessions for current user
    [HttpGet]
    [Authorize(Policy = Permissions.UserManagement.View)]
    public async Task<ActionResult<ApiResponse<List<SessionDto>>>> GetSessions()
    {
        // Get all non-revoked tokens for current user
        var revokedJtis = await _context.Set<Domain.Entities.RevokedToken>()
            .Where(r => r.UserId == CurrentUserId)
            .Select(r => r.Jti)
            .ToListAsync();

        var users = await _context.Users
            .Where(u => u.Id == CurrentUserId && u.RefreshToken != null)
            .ToListAsync();

        var sessions = users.Select(u => new SessionDto
        {
            Id = u.Id,
            Jti = CurrentJti,
            UserId = u.Id,
            UserName = $"{u.FirstName} {u.LastName}",
            UserEmail = u.Email,
            IpAddress = u.LastKnownIp ?? "Current",
            DeviceInfo = u.LastKnownDevice ?? "Current Device",
            LoginTime = u.LastLoginAt ?? DateTime.UtcNow,
            ExpiresAt = u.RefreshTokenExpiryTime ?? DateTime.UtcNow.AddDays(7),
            IsCurrentSession = true
        }).ToList();

        return Ok(ApiResponse<List<SessionDto>>.Ok(sessions, "Sessions retrieved"));
    }

    // DELETE /api/v1/sessions/{id} - Revoke specific session
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.UserManagement.Update)]
    public async Task<ActionResult<ApiResponse<bool>>> RevokeSession(Guid id, [FromBody] RevokeSessionDto request)
    {
        var token = await _context.Set<Domain.Entities.RevokedToken>()
            .FirstOrDefaultAsync(r => r.Jti == request.Jti);

        if (token != null)
            return ApiResponse<bool>.Fail("Session already revoked");

        // Add to blacklist
        _context.Set<Domain.Entities.RevokedToken>().Add(new Domain.Entities.RevokedToken
        {
            Id = Guid.NewGuid(),
            Jti = request.Jti ?? id.ToString(),
            UserId = id,
            RevokedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown",
            RevokedAt = DateTime.UtcNow,
            Reason = request.Reason,
            ExpiresAt = DateTime.UtcNow.AddDays(7), // Auto-cleanup after token would've expired
            CreatedAt = DateTime.UtcNow,
            CreatedBy = CurrentUserId.ToString(),
            TenantId = Guid.Empty,
            IsDeleted = false
        });

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Session revoked successfully");
    }

    // DELETE /api/v1/sessions/all - Revoke all sessions except current
    [HttpDelete("all")]
    [Authorize(Policy = Permissions.UserManagement.Update)]
    public async Task<ActionResult<ApiResponse<bool>>> RevokeAllSessions()
    {
        var user = await _context.Users.FindAsync(CurrentUserId);
        if (user == null) return ApiResponse<bool>.Fail("User not found");

        // Clear refresh token forces all other sessions to expire
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "All other sessions revoked");
    }
}