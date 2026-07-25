using ContractorMonitoring.Domain.Entities.Base;
using ContractorMonitoring.Domain.Enums;

namespace ContractorMonitoring.Domain.Entities;

// User entity for authentication and authorization
public class User : AuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public string? RefreshTokenFamily { get; set; }  // Token family for reuse-detection

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    // User entity
    public string? ProfilePicture { get; set; }
    public string? Bio { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
    public string? Timezone { get; set; }
    public string? Language { get; set; } = "en";
    public string? Theme { get; set; } = "light";
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }
    public DateTime? LastPasswordChange { get; set; }
    public DateTime? LastProfileUpdate { get; set; }
    public string? SecurityQuestion { get; set; }
    public string? SecurityAnswerHash { get; set; }
    public int LoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public string? LastKnownIp { get; set; }
    public string? LastKnownDevice { get; set; }
    public bool MustChangePassword { get; set; }
    public bool IsApproved { get; set; } = true;
    public List<UserSession> Sessions { get; set; } = new();
    public List<UserActivity> Activities { get; set; } = new();
}
