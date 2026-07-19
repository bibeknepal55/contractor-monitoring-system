namespace ContractorMonitoring.Application.Interfaces;

// Centralized permission resolution service
// Single source of truth for "what can this user do?"
public interface IPermissionResolver
{
    Task<List<string>> GetUserRolesAsync(Guid userId);
    Task<List<string>> GetUserPermissionsAsync(Guid userId);
    Task<bool> HasPermissionAsync(Guid userId, string permission);
    Task<bool> HasRoleAsync(Guid userId, string role);
}