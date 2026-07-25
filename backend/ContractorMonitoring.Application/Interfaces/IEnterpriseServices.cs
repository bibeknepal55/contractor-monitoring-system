namespace ContractorMonitoring.Application.Interfaces;

// Phase 1: TOTP MFA
public interface IMfaService
{
    string GenerateSecret();
    string GenerateQrCodeUri(string email, string secret, string issuer = "ContractorMonitoring");
    string GenerateQrCodeBase64(string email, string secret);
    bool ValidateTotp(string secret, string code);
    string[] GenerateBackupCodes(int count = 8);
}

// Phase 1: Audit trail with hash chain
public interface IAuditTrailService
{
    Task LogAsync(string entityName, Guid entityId, string action,
        string? oldValues, string? newValues, Guid? userId, string userEmail, string ipAddress);
    Task<bool> VerifyChainIntegrityAsync(Guid tenantId);
}

// Phase 1: GDPR
public interface IGdprService
{
    Task<string> ExportUserDataAsync(Guid userId);
    Task EraseUserDataAsync(Guid userId, string erasedBy);
}

// Phase 1: IP allowlist / geo-blocking
public interface ISecurityPolicyService
{
    Task<bool> IsIpAllowedAsync(Guid tenantId, string ipAddress);
    Task<bool> IsCountryAllowedAsync(Guid tenantId, string ipAddress);
    string? GetCountryFromIp(string ipAddress);
}

// Phase 2: Tenant management
public interface ITenantManagementService
{
    Task<Domain.Entities.Tenant?> GetTenantBySubdomainAsync(string subdomain);
    Task<Domain.Entities.Tenant?> GetTenantByIdAsync(Guid tenantId);
    Task<Domain.Entities.Tenant> CreateTenantAsync(string name, string subdomain, string adminEmail);
}

// Phase 2: Redis permission cache
public interface IPermissionCacheService
{
    Task<List<string>?> GetCachedPermissionsAsync(Guid userId);
    Task SetCachedPermissionsAsync(Guid userId, List<string> permissions);
    Task InvalidateAsync(Guid userId);
    Task InvalidateAllAsync();
}

// Phase 3: ABAC
public interface IAbacService
{
    Task<bool> EvaluatePolicyAsync(Guid userId, string resource, string action, Dictionary<string, string> attributes);
}

// Phase 3: Real-time permission push
public interface IPermissionBroadcastService
{
    Task BroadcastPermissionChangeAsync(Guid userId);
    Task BroadcastRoleChangeAsync(Guid roleId);
}

// Phase 4: BI / scoring
public interface IPerformanceScoringService
{
    Task ComputeAllScoresAsync();
    Task<decimal> ComputeContractorScoreAsync(Guid contractorId);
}

public interface IPredictiveAlertService
{
    Task EvaluateProjectsAsync();
}

// Phase 5: Notifications (in-app + email + SMS)
public interface INotificationDispatcher
{
    Task SendInAppAsync(Guid userId, string subject, string body, string eventType);
    Task SendEmailAsync(string toAddress, string subject, string body, string eventType);
    Task SendSmsAsync(string phoneNumber, string message);
}

// Phase 6: Health check UI data
public interface IHealthSummaryService
{
    Task<object> GetSummaryAsync();
}
