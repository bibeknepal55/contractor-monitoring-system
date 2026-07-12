using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Application.Interfaces;

// JWT token generation and validation service
public interface IJwtService
{
    Task<string> GenerateAccessToken(User user);
    Task<string> GenerateRefreshToken();
    Task<(bool isValid, Guid userId)> ValidateToken(string token);
    Task<DateTime> GetTokenExpiryTime(string token);
}