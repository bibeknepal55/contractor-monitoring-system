using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

public class UserSession : AuditableEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string DeviceInfo { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime LastActivity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime ExpiresAt { get; set; }

    public User User { get; set; } = null!;
}