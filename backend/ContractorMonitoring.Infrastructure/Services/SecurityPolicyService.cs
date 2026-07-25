using System.Net;
using MaxMind.GeoIP2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Infrastructure.Data;

namespace ContractorMonitoring.Infrastructure.Services;

public class SecurityPolicyService : ISecurityPolicyService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SecurityPolicyService> _logger;
    private readonly string? _geoDbPath;

    public SecurityPolicyService(
        ApplicationDbContext context,
        ILogger<SecurityPolicyService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _geoDbPath = configuration["GeoIP:DatabasePath"]
            ?? Path.Combine(AppContext.BaseDirectory, "GeoLite2-City.mmdb");
    }

    public async Task<bool> IsIpAllowedAsync(Guid tenantId, string ipAddress)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
        if (tenant == null || string.IsNullOrWhiteSpace(tenant.IpAllowlist)) return true;

        var allowedIps = tenant.IpAllowlist.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (allowedIps.Length == 0) return true;

        if (!IPAddress.TryParse(ipAddress, out var requestIp)) return false;

        foreach (var entry in allowedIps)
        {
            if (entry.Contains('/'))
            {
                if (IsInCidr(requestIp, entry)) return true;
            }
            else if (IPAddress.TryParse(entry, out var allowedIp) && requestIp.Equals(allowedIp))
            {
                return true;
            }
        }
        return false;
    }

    public async Task<bool> IsCountryAllowedAsync(Guid tenantId, string ipAddress)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
        if (tenant == null || !tenant.GeoBlockEnabled) return true;
        if (string.IsNullOrWhiteSpace(tenant.AllowedCountries)) return true;

        var country = GetCountryFromIp(ipAddress);
        if (country == null)
        {
            // Cannot determine country — allow by default (fail-open)
            _logger.LogWarning("Could not determine country for IP {IP}; allowing by default", ipAddress);
            return true;
        }

        var allowed = tenant.AllowedCountries
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(c => string.Equals(c, country, StringComparison.OrdinalIgnoreCase));

        if (!allowed)
            _logger.LogWarning("Geo-blocked: IP {IP} resolved to country {Country}, not in allowlist", ipAddress, country);

        return allowed;
    }

    public string? GetCountryFromIp(string ipAddress)
    {
        if (!File.Exists(_geoDbPath))
        {
            _logger.LogDebug("GeoIP database not found at {Path}; geo-blocking disabled", _geoDbPath);
            return null;
        }

        // Loopback / private ranges — treat as local
        if (IPAddress.TryParse(ipAddress, out var ip) && (IPAddress.IsLoopback(ip) || IsPrivateIp(ip)))
            return "LOCAL";

        try
        {
            using var reader = new DatabaseReader(_geoDbPath);
            if (reader.TryCity(ipAddress, out var response))
                return response?.Country?.IsoCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GeoIP lookup failed for IP {IP}", ipAddress);
        }

        return null;
    }

    private static bool IsPrivateIp(IPAddress ip)
    {
        var bytes = ip.GetAddressBytes();
        if (bytes.Length != 4) return false;
        return bytes[0] == 10
            || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
            || (bytes[0] == 192 && bytes[1] == 168);
    }

    private static bool IsInCidr(IPAddress ip, string cidr)
    {
        try
        {
            var parts = cidr.Split('/');
            if (parts.Length != 2) return false;
            if (!IPAddress.TryParse(parts[0], out var network)) return false;
            if (!int.TryParse(parts[1], out var prefixLength)) return false;

            var ipBytes = ip.GetAddressBytes();
            var networkBytes = network.GetAddressBytes();
            if (ipBytes.Length != networkBytes.Length) return false;

            var fullBytes = prefixLength / 8;
            var remainingBits = prefixLength % 8;

            for (int i = 0; i < fullBytes; i++)
                if (ipBytes[i] != networkBytes[i]) return false;

            if (remainingBits > 0)
            {
                var mask = (byte)(0xFF << (8 - remainingBits));
                if ((ipBytes[fullBytes] & mask) != (networkBytes[fullBytes] & mask)) return false;
            }
            return true;
        }
        catch { return false; }
    }
}
