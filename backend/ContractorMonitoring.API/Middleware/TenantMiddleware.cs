using System.Security.Claims;

namespace ContractorMonitoring.API.Middleware;

// Middleware to extract TenantId from JWT claims OR subdomain
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Try JWT claim first (authenticated requests)
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("TenantId");
            if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            {
                context.Items["TenantId"] = tenantId;
                await _next(context);
                return;
            }
        }

        // 2. Phase 2: Try subdomain routing (e.g. pwdnepal.cms.gov)
        var host = context.Request.Host.Host;
        var parts = host.Split('.');
        if (parts.Length >= 3)
        {
            var subdomain = parts[0].ToLower();
            if (subdomain != "www" && subdomain != "api")
            {
                context.Items["Subdomain"] = subdomain;
                // TenantManagementService resolves subdomain → TenantId at query time
            }
        }

        await _next(context);
    }
}