using System.Security.Claims;

namespace ContractorMonitoring.API.Middleware;

// Middleware to extract TenantId from JWT claims
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("TenantId");
            if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            {
                context.Items["TenantId"] = tenantId;
            }
        }

        await _next(context);
    }
}