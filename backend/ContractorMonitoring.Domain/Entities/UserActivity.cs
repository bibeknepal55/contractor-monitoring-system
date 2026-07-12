using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

public class UserActivity : AuditableEntity
{
    public Guid UserId { get; set; }
    public string ActivityType { get; set; } = string.Empty; // Login, Logout, ProfileUpdate, PasswordChange, etc.
    public string Description { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string DeviceInfo { get; set; } = string.Empty;
    public string? Metadata { get; set; } // JSON for extra data

    public User User { get; set; } = null!;
}