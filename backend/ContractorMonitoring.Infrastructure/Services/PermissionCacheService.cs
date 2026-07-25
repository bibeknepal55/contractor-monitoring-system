using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Infrastructure.Services;

public class PermissionCacheService : IPermissionCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<PermissionCacheService> _logger;
    private readonly IConnectionMultiplexer? _redis;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(15);

    public PermissionCacheService(
        IDistributedCache cache,
        ILogger<PermissionCacheService> logger,
        IConfiguration configuration)
    {
        _cache = cache;
        _logger = logger;

        var redisConn = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConn))
        {
            try { _redis = ConnectionMultiplexer.Connect(redisConn); }
            catch (Exception ex) { _logger.LogWarning(ex, "Redis connection failed; bulk invalidation will be no-op"); }
        }
    }

    private static string Key(Guid userId) => $"perms:{userId}";

    public async Task<List<string>?> GetCachedPermissionsAsync(Guid userId)
    {
        try
        {
            var json = await _cache.GetStringAsync(Key(userId));
            return json == null ? null : JsonSerializer.Deserialize<List<string>>(json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis cache read failed for user {UserId}, falling back to DB", userId);
            return null;
        }
    }

    public async Task SetCachedPermissionsAsync(Guid userId, List<string> permissions)
    {
        try
        {
            var json = JsonSerializer.Serialize(permissions);
            await _cache.SetStringAsync(Key(userId), json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheTtl
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis cache write failed for user {UserId}", userId);
        }
    }

    public async Task InvalidateAsync(Guid userId)
    {
        try { await _cache.RemoveAsync(Key(userId)); }
        catch (Exception ex) { _logger.LogWarning(ex, "Redis cache invalidation failed for user {UserId}", userId); }
    }

    public async Task InvalidateAllAsync()
    {
        if (_redis == null)
        {
            _logger.LogWarning("Redis not available; bulk permission cache invalidation skipped");
            return;
        }

        try
        {
            var db = _redis.GetDatabase();
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var deleted = 0;

            await foreach (var key in server.KeysAsync(pattern: "perms:*"))
            {
                await db.KeyDeleteAsync(key);
                deleted++;
            }

            _logger.LogInformation("Bulk permission cache invalidation: {Count} keys deleted", deleted);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Bulk Redis cache invalidation failed");
        }
    }
}
