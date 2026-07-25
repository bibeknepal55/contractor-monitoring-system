using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Domain.Entities;
using ContractorMonitoring.Infrastructure.Data;

namespace ContractorMonitoring.Infrastructure.Services;

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _email;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly IConfiguration _config;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        ApplicationDbContext db, IEmailService email,
        IHubContext<NotificationHub> hub, IConfiguration config,
        ILogger<NotificationDispatcher> logger)
    {
        _db = db; _email = email; _hub = hub; _config = config; _logger = logger;
    }

    public async Task SendInAppAsync(Guid userId, string subject, string body, string eventType)
    {
        var log = new NotificationLog
        {
            Id = Guid.NewGuid(), UserId = userId, Type = "InApp",
            EventType = eventType, Subject = subject, Body = body,
            Status = "Sent", SentAt = DateTime.UtcNow, IsRead = false,
            CreatedAt = DateTime.UtcNow, CreatedBy = "System", TenantId = Guid.Empty
        };
        _db.NotificationLogs.Add(log);
        await _db.SaveChangesAsync();

        // Push via SignalR
        await _hub.Clients.Group($"user_{userId}").SendAsync("NewNotification", new
        {
            id = log.Id, subject, body, eventType, sentAt = log.SentAt
        });
    }

    public async Task SendEmailAsync(string toAddress, string subject, string body, string eventType)
        => await _email.SendAsync(subject, body, eventType, toAddress);

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        // Twilio integration — reads from config
        var accountSid = _config["Twilio:AccountSid"];
        var authToken  = _config["Twilio:AuthToken"];
        var fromNumber = _config["Twilio:FromNumber"];

        if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken))
        {
            _logger.LogDebug("SMS skipped: Twilio not configured");
            return;
        }

        // Production: use Twilio SDK
        // Twilio.TwilioClient.Init(accountSid, authToken);
        // await MessageResource.CreateAsync(to: new PhoneNumber(phoneNumber), from: new PhoneNumber(fromNumber), body: message);
        _logger.LogInformation("SMS would be sent to {Phone}: {Message}", phoneNumber, message);
        await Task.CompletedTask;
    }
}

// SignalR hub for real-time in-app notifications
public class NotificationHub : Hub
{
    public async Task JoinUserGroup(string userId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

    public async Task MarkRead(string notificationId)
        => await Clients.Caller.SendAsync("NotificationRead", notificationId);
}
