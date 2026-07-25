using ContractorMonitoring.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ContractorMonitoring.Infrastructure.Services;

public interface ITenantService
{
    Guid? CurrentTenantId { get; }
}

// Scoped service — reads TenantId from HttpContext at query time, not model-creation time
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? CurrentTenantId
    {
        get
        {
            if (_httpContextAccessor.HttpContext?.Items["TenantId"] is Guid tenantId)
                return tenantId;
            return null;
        }
    }
}
