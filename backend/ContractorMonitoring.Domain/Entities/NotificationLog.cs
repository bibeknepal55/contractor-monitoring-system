using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Record of every notification sent
public class NotificationLog : AuditableEntity
{
    public Guid? UserId { get; set; }
    public string Type { get; set; } = string.Empty;       // "Email", "InApp", "Webhook"
    public string EventType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;     // "Sent", "Failed", "Pending"
    public DateTime? SentAt { get; set; }
    public bool IsRead { get; set; } = false;
}