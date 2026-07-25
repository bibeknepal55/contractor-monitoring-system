namespace ContractorMonitoring.Application.DTOs.Auth;

// Authentication response DTO
public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string ExpiresAt { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

// User DTO for responses
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public bool MustChangePassword { get; set; }
}