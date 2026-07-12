using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Tracks every API request for complete forensic audit trail
// Accessible only by SuperAdmin and Admin users
public class UserActivityLog : AuditableEntity
{
    // User Information (nullable for anonymous/failed requests)
    public Guid? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;

    // Activity Classification
    public string ActivityType { get; set; } = string.Empty;  // Login, Logout, Create, Update, Delete, View, Export, FailedLogin, AccessDenied, Error
    public string ModuleName { get; set; } = string.Empty;    // Projects, Contractors, PerformanceBonds, Auth, etc.
    public string Action { get; set; } = string.Empty;        // "Created Project PRJ-001", "Updated Performance Bond PB-002"
    public string? Description { get; set; }                  // Full human-readable description of the activity

    // Request Technical Details
    public string IpAddress { get; set; } = string.Empty;
    public string? Location { get; set; }                     // City, Country - from IP geolocation service
    public string? DeviceInfo { get; set; }                   // "Chrome 149 on Windows 10"
    public string? UserAgent { get; set; }                    // Raw User-Agent header string
    public string RequestMethod { get; set; } = string.Empty; // GET, POST, PUT, DELETE, PATCH
    public string RequestUrl { get; set; } = string.Empty;    // Full request path with query string
    public string? RequestBody { get; set; }                  // JSON payload for POST/PUT (truncated)
    public int ResponseStatus { get; set; }                   // HTTP status code (200, 400, 401, 403, 500)

    // Session Tracking
    public string? SessionId { get; set; }                    // Groups multiple requests into one session
    public DateTime? LoginTimestamp { get; set; }             // When the user logged in
    public DateTime? LogoutTimestamp { get; set; }            // When the user logged out
    public int? SessionDuration { get; set; }                 // Session duration in seconds

    // Navigation property
    public User? User { get; set; }
}