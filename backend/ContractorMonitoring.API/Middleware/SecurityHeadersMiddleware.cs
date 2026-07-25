using System.Security.Claims;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.API.Middleware;

// Phase 1: Penetration-test hardening — security headers + IP allowlist + geo-blocking
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Content Security Policy
        headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
            "font-src 'self' https://fonts.gstatic.com; " +
            "img-src 'self' data: blob:; " +
            "connect-src 'self' ws: wss:; " +
            "frame-ancestors 'none';";

        // Prevent clickjacking
        headers["X-Frame-Options"] = "DENY";

        // Prevent MIME sniffing
        headers["X-Content-Type-Options"] = "nosniff";

        // XSS protection (legacy browsers)
        headers["X-XSS-Protection"] = "1; mode=block";

        // Referrer policy
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Permissions policy
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";

        // HSTS (only on HTTPS)
        if (context.Request.IsHttps)
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";

        await _next(context);
    }
}

// Phase 1: IP allowlist + geo-blocking enforcement
public class IpSecurityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpSecurityMiddleware> _logger;

    public IpSecurityMiddleware(RequestDelegate next, ILogger<IpSecurityMiddleware> logger)
    {
        _next = next; _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ISecurityPolicyService securityPolicy)
    {
        // Only enforce for authenticated requests with a tenant
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("TenantId")?.Value;
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim, out var tenantId) && tenantId != Guid.Empty)
            {
                var ipAllowed = await securityPolicy.IsIpAllowedAsync(tenantId, ipAddress);
                if (!ipAllowed)
                {
                    _logger.LogWarning("IP {IP} blocked for tenant {TenantId}", ipAddress, tenantId);
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new { success = false, message = "Access denied: IP not in allowlist" });
                    return;
                }

                var countryAllowed = await securityPolicy.IsCountryAllowedAsync(tenantId, ipAddress);
                if (!countryAllowed)
                {
                    _logger.LogWarning("Country blocked for IP {IP}, tenant {TenantId}", ipAddress, tenantId);
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new { success = false, message = "Access denied: country not allowed" });
                    return;
                }
            }
        }

        await _next(context);
    }
}
