using System.Text.Json;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ContractorMonitoring.Infrastructure.Services;

namespace ContractorMonitoring.UnitTests.Services;

public class PermissionCacheServiceTests
{
    private readonly Mock<IDistributedCache> _cache = new();
    private readonly Mock<ILogger<PermissionCacheService>> _logger = new();
    private readonly Mock<IConfiguration> _config = new();

    private PermissionCacheService CreateService()
    {
        _config.Setup(c => c.GetSection("ConnectionStrings")["Redis"]).Returns((string?)null);
        return new PermissionCacheService(_cache.Object, _logger.Object, _config.Object);
    }

    [Fact]
    public async Task GetCachedPermissionsAsync_CacheHit_ReturnsPermissions()
    {
        var userId = Guid.NewGuid();
        var perms = new List<string> { "projects.read", "projects.write" };
        var json = JsonSerializer.SerializeToUtf8Bytes(perms);
        _cache.Setup(c => c.GetAsync($"perms:{userId}", It.IsAny<CancellationToken>()))
              .ReturnsAsync(json);

        var result = await CreateService().GetCachedPermissionsAsync(userId);

        result.Should().BeEquivalentTo(perms);
    }

    [Fact]
    public async Task GetCachedPermissionsAsync_CacheMiss_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        _cache.Setup(c => c.GetAsync($"perms:{userId}", It.IsAny<CancellationToken>()))
              .ReturnsAsync((byte[]?)null);

        var result = await CreateService().GetCachedPermissionsAsync(userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetCachedPermissionsAsync_StoresSerializedPermissions()
    {
        var userId = Guid.NewGuid();
        var perms = new List<string> { "admin.access" };
        byte[]? stored = null;
        _cache.Setup(c => c.SetAsync(
                $"perms:{userId}",
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
              .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                  (_, bytes, _, _) => stored = bytes)
              .Returns(Task.CompletedTask);

        await CreateService().SetCachedPermissionsAsync(userId, perms);

        stored.Should().NotBeNull();
        var deserialized = JsonSerializer.Deserialize<List<string>>(stored!);
        deserialized.Should().BeEquivalentTo(perms);
    }

    [Fact]
    public async Task InvalidateAsync_RemovesKey()
    {
        var userId = Guid.NewGuid();
        _cache.Setup(c => c.RemoveAsync($"perms:{userId}", It.IsAny<CancellationToken>()))
              .Returns(Task.CompletedTask);

        await CreateService().InvalidateAsync(userId);

        _cache.Verify(c => c.RemoveAsync($"perms:{userId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCachedPermissionsAsync_CacheThrows_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        _cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ThrowsAsync(new Exception("Redis down"));

        var result = await CreateService().GetCachedPermissionsAsync(userId);

        result.Should().BeNull();
    }
}
