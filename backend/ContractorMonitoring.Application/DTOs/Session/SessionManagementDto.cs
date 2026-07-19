namespace ContractorMonitoring.Application.DTOs.Session;

public class SessionDto
{
    public Guid Id { get; set; }
    public string Jti { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string DeviceInfo { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsCurrentSession { get; set; }
}

public class RevokeSessionDto
{
    public string? Jti { get; set; }
    public string? Reason { get; set; }
}