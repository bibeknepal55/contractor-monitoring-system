namespace ContractorMonitoring.Application.DTOs.Profile;

// Complete profile response
public class ProfileDto
{
    // Basic Info
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Bio { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }

    // Roles & Permissions
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();

    // Account Status
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastProfileUpdate { get; set; }
    public DateTime CreatedAt { get; set; }

    // Preferences
    public string Timezone { get; set; } = "UTC";
    public string Language { get; set; } = "en";
    public string Theme { get; set; } = "light";

    // Notification Settings
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; }

    // Security
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LastPasswordChange { get; set; }
    public bool HasSecurityQuestion { get; set; }

    // Sessions
    public List<SessionDto> ActiveSessions { get; set; } = new();

    // Recent Activity
    public List<ActivityDto> RecentActivities { get; set; } = new();
}

public class SessionDto
{
    public Guid Id { get; set; }
    public string DeviceInfo { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime LastActivity { get; set; }
    public bool IsCurrent { get; set; }
}

public class ActivityDto
{
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Update profile request
public class UpdateProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
}

// Update preferences request
public class UpdatePreferencesDto
{
    public string? Timezone { get; set; }
    public string? Language { get; set; }
    public string? Theme { get; set; }
    public bool EmailNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public bool SmsNotifications { get; set; }
}

// Change email request
public class ChangeEmailDto
{
    public string NewEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Change phone request
public class ChangePhoneDto
{
    public string NewPhone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Setup security question
public class SecurityQuestionDto
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Setup 2FA
public class TwoFactorSetupDto
{
    public bool Enable { get; set; }
    public string Password { get; set; } = string.Empty;
}

// Revoke session
public class RevokeSessionDto
{
    public Guid SessionId { get; set; }
}

// Upload profile picture
public class ProfilePictureDto
{
    public string PictureUrl { get; set; } = string.Empty;
}

// Profile picture download DTO 
public class ProfilePictureDownloadDto
{
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "image/jpeg";
}