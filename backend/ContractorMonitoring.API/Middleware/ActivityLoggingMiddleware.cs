using System.Security.Claims;
using System.Text;
using ContractorMonitoring.Domain.Entities;
using ContractorMonitoring.Infrastructure.Data;
using Microsoft.AspNetCore.Http.Extensions;

namespace ContractorMonitoring.API.Middleware;

// Enterprise Activity Logging Middleware
public class ActivityLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ActivityLoggingMiddleware> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    // Paths excluded from logging to reduce noise
    private static readonly string[] ExcludedPaths =
    {
        "/swagger", "/health", "/favicon.ico", "/debug", "/_framework",
        "/api/v1/user-logs"  // Don't log viewing the audit log itself
    };

    public ActivityLoggingMiddleware(
        RequestDelegate next,
        ILogger<ActivityLoggingMiddleware> logger,
        IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == "OPTIONS")
        {
            await _next(context);
            return;
        }

        if (ExcludedPaths.Any(p => context.Request.Path.StartsWithSegments(p)))
        {
            await _next(context);
            return;
        }

        string? requestBody = null;
        var isAuthEndpoint = context.Request.Path.StartsWithSegments("/api/v1/auth");
        var isCrudOperation = context.Request.Method is "POST" or "PUT" or "PATCH";

        if (isCrudOperation && !isAuthEndpoint)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            if (requestBody.Length > 5000)
                requestBody = string.Concat(requestBody.AsSpan(0, 5000), "... [truncated]");
            context.Request.Body.Position = 0;
        }

        if (isAuthEndpoint && context.Request.Path.Value?.Contains("/login") == true && isCrudOperation)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
            requestBody = "Login attempt";
        }

        // Capture response body
        var originalBodyStream = context.Response.Body;
        using var responseBodyBuffer = new MemoryStream();
        context.Response.Body = responseBodyBuffer;

        await _next(context);

        var statusCode = context.Response.StatusCode;

        // Copy response back BEFORE firing background work
        responseBodyBuffer.Seek(0, SeekOrigin.Begin);
        await responseBodyBuffer.CopyToAsync(originalBodyStream);
        context.Response.Body = originalBodyStream;

        // Capture all values needed for logging before async work
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userEmail = context.User.FindFirst(ClaimTypes.Email)?.Value ?? "anonymous";
        var userFirstName = context.User.FindFirst(ClaimTypes.GivenName)?.Value ?? "";
        var userLastName = context.User.FindFirst(ClaimTypes.Surname)?.Value ?? "";
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value
                       ?? context.User.FindFirst("Role")?.Value ?? "Anonymous";
        var tenantId = context.User.FindFirst("TenantId")?.Value;
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var requestMethod = context.Request.Method;
        var requestUrl = context.Request.GetDisplayUrl();
        var requestPath = context.Request.Path;
        var sessionId = context.Request.Headers["X-Session-Id"].FirstOrDefault();

        // Safe fire-and-forget using captured values (no HttpContext access after this point)
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var activityType = DetermineActivityType(requestMethod, requestPath, statusCode);
                var moduleName = DetermineModuleName(requestPath);
                var action = DetermineAction(requestMethod, requestPath, moduleName);
                var deviceInfo = ParseDeviceInfo(userAgent);

                var log = new UserActivityLog
                {
                    Id = Guid.NewGuid(),
                    UserId = userId != null ? Guid.Parse(userId) : null,
                    UserName = $"{userFirstName} {userLastName}".Trim(),
                    UserEmail = userEmail,
                    UserRole = userRole,
                    ActivityType = activityType,
                    ModuleName = moduleName,
                    Action = action,
                    Description = $"[{statusCode}] {activityType} - {moduleName} - {action}",
                    IpAddress = ipAddress,
                    Location = null,
                    DeviceInfo = deviceInfo,
                    UserAgent = userAgent,
                    RequestMethod = requestMethod,
                    RequestUrl = requestUrl,
                    RequestBody = requestBody,
                    ResponseStatus = statusCode,
                    SessionId = sessionId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId ?? "system",
                    TenantId = tenantId != null ? Guid.Parse(tenantId) : Guid.Empty
                };

                dbContext.UserActivityLogs.Add(log);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log user activity");
            }
        });
    }

    // Activity type detection
    // ONLY auth endpoints get Login/Logout/FailedLogin
    private static string DetermineActivityType(string method, PathString path, int status)
    {
        var pathStr = path.ToString();
        var isAuthEndpoint = pathStr.StartsWith("/api/v1/auth", StringComparison.OrdinalIgnoreCase);

        // Auth-specific classifications
        if (isAuthEndpoint)
        {
            if (pathStr.Contains("/login", StringComparison.OrdinalIgnoreCase))
            {
                if (status == 200) return "Login";
                if (status == 401) return "FailedLogin";
                return "FailedLogin";
            }
            if (pathStr.Contains("/logout", StringComparison.OrdinalIgnoreCase)) return "Logout";
            if (pathStr.Contains("/register", StringComparison.OrdinalIgnoreCase)) return "Register";
            if (pathStr.Contains("/refresh-token", StringComparison.OrdinalIgnoreCase)) return "TokenRefresh";
            if (pathStr.Contains("/change-password", StringComparison.OrdinalIgnoreCase)) return "PasswordChange";
        }

        // Status-based classifications for non-auth endpoints
        if (status == 401) return "AccessDenied";
        if (status == 403) return "AccessDenied";
        if (status >= 500) return "Error";

        // Method-based classifications for all other endpoints
        if (pathStr.Contains("/export", StringComparison.OrdinalIgnoreCase)) return "Export";
        if (pathStr.Contains("/upload", StringComparison.OrdinalIgnoreCase)) return "Upload";
        if (pathStr.Contains("/download", StringComparison.OrdinalIgnoreCase)) return "Download";
        if (pathStr.Contains("/purge", StringComparison.OrdinalIgnoreCase)) return "Purge";

        return method switch
        {
            "GET" => "View",
            "POST" => "Create",
            "PUT" => "Update",
            "PATCH" => "Update",
            "DELETE" => "Delete",
            _ => "Other"
        };
    }

    // Module detection from URL path
    private static string DetermineModuleName(PathString path)
    {
        var pathLower = path.ToString().ToLowerInvariant();

        if (pathLower.Contains("/auth")) return "Auth";
        if (pathLower.Contains("/profile")) return "Profile";
        if (pathLower.Contains("/user-logs")) return "UserLogs";
        if (pathLower.Contains("/users")) return "UserManagement";
        if (pathLower.Contains("/dashboard")) return "Dashboard";
        if (pathLower.Contains("/projects")) return "Projects";
        if (pathLower.Contains("/contractors")) return "Contractors";
        if (pathLower.Contains("/contract-financial")) return "ContractFinancials";
        if (pathLower.Contains("/price-adjust")) return "PriceAdjustments";
        if (pathLower.Contains("/performance-bond")) return "PerformanceBonds";
        if (pathLower.Contains("/advance-payment")) return "AdvancePaymentGuarantees";
        if (pathLower.Contains("/physical-progress")) return "PhysicalProgress";
        if (pathLower.Contains("/time-extension")) return "TimeExtensions";
        if (pathLower.Contains("/delay")) return "DelayReasons";
        if (pathLower.Contains("/raw-material")) return "RawMaterials";
        if (pathLower.Contains("/lab-test")) return "LabTests";
        if (pathLower.Contains("/photo")) return "PhotoMonitoring";
        if (pathLower.Contains("/subcontractor")) return "Subcontractors";
        if (pathLower.Contains("/responsible")) return "ResponsibleOfficials";
        if (pathLower.Contains("/report")) return "Reports";
        if (pathLower.Contains("/export")) return "Export";
        if (pathLower.Contains("/approval")) return "ApprovalWorkflow";

        return "General";
    }

    // Meaningful action descriptions
    private static string DetermineAction(string method, PathString path, string moduleName)
    {
        var pathStr = path.ToString();
        var segments = pathStr.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Handle special cases
        if (pathStr.Contains("/login", StringComparison.OrdinalIgnoreCase)) return "User logged in";
        if (pathStr.Contains("/logout", StringComparison.OrdinalIgnoreCase)) return "User logged out";
        if (pathStr.Contains("/register", StringComparison.OrdinalIgnoreCase)) return "New user registered";
        if (pathStr.Contains("/change-password", StringComparison.OrdinalIgnoreCase)) return "Password changed";
        if (pathStr.Contains("/refresh-token", StringComparison.OrdinalIgnoreCase)) return "Token refreshed";
        if (pathStr.Contains("/export", StringComparison.OrdinalIgnoreCase)) return "Exported data";
        if (pathStr.Contains("/upload", StringComparison.OrdinalIgnoreCase)) return "Uploaded file";
        if (pathStr.Contains("/download", StringComparison.OrdinalIgnoreCase)) return "Downloaded file";
        if (pathStr.Contains("/purge", StringComparison.OrdinalIgnoreCase)) return "Cleared old logs";
        if (pathStr.Contains("/stats", StringComparison.OrdinalIgnoreCase)) return "Viewed statistics";
        if (pathStr.Contains("/sessions/active", StringComparison.OrdinalIgnoreCase)) return "Viewed active sessions";

        // Extract resource name from URL
        var resource = "record";
        if (segments.Length >= 3)
        {
            resource = segments[^1];
            if (Guid.TryParse(resource, out _) && segments.Length >= 4)
                resource = segments[^2];
        }

        var verb = method switch
        {
            "GET" => "Listed",
            "POST" => "Created",
            "PUT" => "Updated",
            "PATCH" => "Modified",
            "DELETE" => "Deleted",
            _ => "Processed"
        };

        return $"{verb} {moduleName.ToLower()} {resource}";
    }

    // Parse User-Agent string into readable device info
    private static string ParseDeviceInfo(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown Device";

        var os = "Unknown OS";
        if (userAgent.Contains("Windows NT 10")) os = "Windows 10/11";
        else if (userAgent.Contains("Windows NT 6.3")) os = "Windows 8.1";
        else if (userAgent.Contains("Windows NT 6.2")) os = "Windows 8";
        else if (userAgent.Contains("Windows NT 6.1")) os = "Windows 7";
        else if (userAgent.Contains("Mac OS X")) os = "macOS";
        else if (userAgent.Contains("Linux") && !userAgent.Contains("Android")) os = "Linux";
        else if (userAgent.Contains("Android")) os = "Android";
        else if (userAgent.Contains("iPhone")) os = "iOS (iPhone)";
        else if (userAgent.Contains("iPad")) os = "iOS (iPad)";

        var browser = "Unknown Browser";
        if (userAgent.Contains("Edg/")) browser = "Microsoft Edge";
        else if (userAgent.Contains("Chrome/") && !userAgent.Contains("Edg/")) browser = "Google Chrome";
        else if (userAgent.Contains("Firefox/")) browser = "Mozilla Firefox";
        else if (userAgent.Contains("Safari/") && !userAgent.Contains("Chrome/")) browser = "Apple Safari";
        else if (userAgent.Contains("OPR/") || userAgent.Contains("Opera/")) browser = "Opera";

        return $"{browser} on {os}".Trim();
    }
}