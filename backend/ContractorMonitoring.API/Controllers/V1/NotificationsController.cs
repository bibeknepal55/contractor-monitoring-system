using System.Security.Claims;
using Asp.Versioning;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Notification;
using ContractorMonitoring.Domain.Constants;
using ContractorMonitoring.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[ApiController]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NotificationsController(ApplicationDbContext context) => _context = context;

    private Guid CurrentUserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);

    [HttpGet("notifications")]
    public async Task<ActionResult<ApiResponse<object>>> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var notifications = await _context.Set<Domain.Entities.NotificationLog>()
            .Where(n => n.UserId == CurrentUserId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                EventType = n.EventType,
                Subject = n.Subject,
                Body = n.Body,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        var unreadCount = await _context.Set<Domain.Entities.NotificationLog>()
            .CountAsync(n => n.UserId == CurrentUserId && !n.IsRead);

        return Ok(ApiResponse<object>.Ok(new { UnreadCount = unreadCount, Notifications = notifications }, "Notifications retrieved"));
    }

    [HttpPut("notifications/{id:guid}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkRead(Guid id)
    {
        var notification = await _context.Set<Domain.Entities.NotificationLog>().FindAsync(id);
        if (notification == null) return ApiResponse<bool>.Fail("Notification not found");

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Marked as read");
    }

    [HttpGet("settings/notifications")]
    public async Task<ActionResult<ApiResponse<NotificationSettingsDto>>> GetSettings()
    {
        var user = await _context.Users.FindAsync(CurrentUserId);
        if (user == null) return ApiResponse<NotificationSettingsDto>.Fail("User not found");

        return ApiResponse<NotificationSettingsDto>.Ok(new NotificationSettingsDto
        {
            EmailNotifications = user.EmailNotifications,
            PushNotifications = user.PushNotifications,
            InAppNotifications = true
        }, "Settings retrieved");
    }

    [HttpPut("settings/notifications")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateSettings([FromBody] NotificationSettingsDto request)
    {
        var user = await _context.Users.FindAsync(CurrentUserId);
        if (user == null) return ApiResponse<bool>.Fail("User not found");

        user.EmailNotifications = request.EmailNotifications;
        user.PushNotifications = request.PushNotifications;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Notification settings updated");
    }

    [HttpGet("settings/webhooks")]
    [Authorize(Policy = Permissions.UserManagement.View)]
    public async Task<ActionResult<ApiResponse<List<WebhookSubscriptionDto>>>> GetWebhooks()
    {
        var webhooks = await _context.Set<Domain.Entities.WebhookSubscription>()
            .Where(w => !w.IsDeleted)
            .Select(w => new WebhookSubscriptionDto
            {
                Id = w.Id,
                Name = w.Name,
                Url = w.Url,
                Events = w.Events,
                IsActive = w.IsActive,
                LastTriggeredAt = w.LastTriggeredAt,
                SuccessCount = w.SuccessCount,
                FailureCount = w.FailureCount
            })
            .ToListAsync();

        return ApiResponse<List<WebhookSubscriptionDto>>.Ok(webhooks, "Webhooks retrieved");
    }

    [HttpPost("settings/webhooks")]
    [Authorize(Policy = Permissions.UserManagement.Create)]
    public async Task<ActionResult<ApiResponse<WebhookSubscriptionDto>>> CreateWebhook([FromBody] CreateWebhookDto request)
    {
        var webhook = new Domain.Entities.WebhookSubscription
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Url = request.Url,
            Secret = Guid.NewGuid().ToString("N"),
            Events = request.Events,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = CurrentUserId.ToString(),
            TenantId = Guid.Empty,
            IsDeleted = false
        };

        _context.Set<Domain.Entities.WebhookSubscription>().Add(webhook);
        await _context.SaveChangesAsync();

        return ApiResponse<WebhookSubscriptionDto>.Ok(new WebhookSubscriptionDto
        {
            Id = webhook.Id,
            Name = webhook.Name,
            Url = webhook.Url,
            Events = webhook.Events,
            IsActive = webhook.IsActive
        }, "Webhook created");
    }

    [HttpDelete("settings/webhooks/{id:guid}")]
    [Authorize(Policy = Permissions.UserManagement.Delete)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteWebhook(Guid id)
    {
        var webhook = await _context.Set<Domain.Entities.WebhookSubscription>().FindAsync(id);
        if (webhook == null) return ApiResponse<bool>.Fail("Webhook not found");

        webhook.IsDeleted = true;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Webhook deleted");
    }
}