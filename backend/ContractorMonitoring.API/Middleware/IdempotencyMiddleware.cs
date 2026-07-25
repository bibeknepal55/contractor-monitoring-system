using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace ContractorMonitoring.API.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly TimeSpan KeyTtl = TimeSpan.FromHours(24);
    private static readonly HashSet<string> IdempotentMethods =
        new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT" };

    public IdempotencyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IDistributedCache cache, ILogger<IdempotencyMiddleware> logger)
    {
        if (!IdempotentMethods.Contains(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var idempotencyKey = context.Request.Headers["Idempotency-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await _next(context);
            return;
        }

        if (idempotencyKey.Length > 128)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Idempotency-Key must be <= 128 characters" });
            return;
        }

        var cacheKey = $"idempotency:{idempotencyKey}";

        // Return cached response on duplicate request
        try
        {
            var cached = await cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                var stored = JsonSerializer.Deserialize<CachedResponse>(cached);
                if (stored != null)
                {
                    logger.LogDebug("Idempotency cache hit for key {Key}", idempotencyKey);
                    context.Response.StatusCode = stored.StatusCode;
                    context.Response.ContentType = stored.ContentType;
                    context.Response.Headers.Append("X-Idempotency-Replayed", "true");
                    await context.Response.WriteAsync(stored.Body);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Idempotency cache read failed; proceeding without idempotency check");
            await _next(context);
            return;
        }

        // Capture response body — ALWAYS restore original stream in finally
        var originalBody = context.Response.Body;
        var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await _next(context);

            // Only cache 2xx responses
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                buffer.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(buffer).ReadToEndAsync();

                try
                {
                    var toCache = JsonSerializer.Serialize(new CachedResponse
                    {
                        StatusCode = context.Response.StatusCode,
                        ContentType = context.Response.ContentType ?? "application/json",
                        Body = responseBody
                    });
                    await cache.SetStringAsync(cacheKey, toCache, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = KeyTtl
                    });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Idempotency cache write failed for key {Key}", idempotencyKey);
                }
            }

            buffer.Seek(0, SeekOrigin.Begin);
            await buffer.CopyToAsync(originalBody);
        }
        finally
        {
            // Always restore — ensures GlobalExceptionMiddleware can write error responses
            context.Response.Body = originalBody;
            await buffer.DisposeAsync();
        }
    }

    private sealed record CachedResponse
    {
        public int StatusCode { get; init; }
        public string ContentType { get; init; } = "application/json";
        public string Body { get; init; } = string.Empty;
    }
}
