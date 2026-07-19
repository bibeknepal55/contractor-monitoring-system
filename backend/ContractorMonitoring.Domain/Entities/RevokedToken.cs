using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Blacklisted JWT tokens - enables real session revocation
public class RevokedToken : AuditableEntity
{
    public string Jti { get; set; } = string.Empty;     // JWT Token ID (jti claim)
    public Guid UserId { get; set; }
    public string RevokedBy { get; set; } = string.Empty;
    public DateTime RevokedAt { get; set; }
    public string? Reason { get; set; }
    public DateTime ExpiresAt { get; set; }              // When the original token would have expired
}