using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Application.Interfaces;

// JWT token generation and validation service
public interface IJwtService
{
    Task<string> GenerateAccessToken(User user, List<string>? roles = null, List<string>? permissions = null);
    Task<string> GenerateRefreshToken();
    Task<(bool isValid, Guid userId)> ValidateToken(string token, bool validateLifetime = true);
    Task<string> GetTokenExpiryTime(string token);

    // Make these public for centralized permission resolution
    Task<List<string>> GetUserRolesAsync(Guid userId);
    Task<List<string>> GetUserPermissionsAsync(Guid userId);
}