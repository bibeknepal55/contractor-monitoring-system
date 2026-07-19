using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Pre-defined notification templates for system events
public class NotificationTemplate : AuditableEntity
{
    public string EventType { get; set; } = string.Empty;    // "ApprovalRequested", "ProjectCreated", etc.
    public string Subject { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty; // {UserName}, {ProjectName} placeholders
    public bool IsActive { get; set; } = true;
}