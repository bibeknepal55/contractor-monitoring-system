using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Domain.Entities;
using ContractorMonitoring.Infrastructure.Data;

namespace ContractorMonitoring.Infrastructure.Services;

public class AuditTrailService : IAuditTrailService
{
    private readonly ApplicationDbContext _context;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public AuditTrailService(ApplicationDbContext context) => _context = context;

    public async Task LogAsync(string entityName, Guid entityId, string action,
        string? oldValues, string? newValues, Guid? userId, string userEmail, string ipAddress)
    {
        await _lock.WaitAsync();
        try
        {
            // Get hash of last entry for this tenant to chain
            var lastHash = await _context.AuditTrails
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => a.CurrentHash)
                .FirstOrDefaultAsync() ?? "GENESIS";

            var entry = new AuditTrail
            {
                Id = Guid.NewGuid(),
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                OldValues = oldValues,
                NewValues = newValues,
                ChangedColumns = ComputeChangedColumns(oldValues, newValues),
                UserId = userId,
                UserEmail = userEmail,
                IpAddress = ipAddress,
                PreviousHash = lastHash,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userEmail,
                TenantId = Guid.Empty
            };

            entry.CurrentHash = ComputeHash(entry, lastHash);

            _context.AuditTrails.Add(entry);
            await _context.SaveChangesAsync();
        }
        finally { _lock.Release(); }
    }

    public async Task<bool> VerifyChainIntegrityAsync(Guid tenantId)
    {
        var entries = await _context.AuditTrails
            .IgnoreQueryFilters()
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();

        if (!entries.Any()) return true;

        string previousHash = "GENESIS";
        foreach (var entry in entries)
        {
            if (entry.PreviousHash != previousHash) return false;
            var expected = ComputeHash(entry, previousHash);
            if (entry.CurrentHash != expected) return false;
            previousHash = entry.CurrentHash;
        }
        return true;
    }

    private static string ComputeHash(AuditTrail entry, string previousHash)
    {
        var content = $"{entry.Id}|{entry.EntityName}|{entry.EntityId}|{entry.Action}|{entry.OldValues}|{entry.NewValues}|{entry.UserId}|{entry.CreatedAt:O}|{previousHash}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes);
    }

    private static string? ComputeChangedColumns(string? oldJson, string? newJson)
    {
        if (oldJson == null || newJson == null) return null;
        try
        {
            var old = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(oldJson);
            var @new = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(newJson);
            if (old == null || @new == null) return null;
            var changed = @new.Keys.Where(k => old.TryGetValue(k, out var ov) && ov.ToString() != @new[k].ToString()).ToList();
            return changed.Any() ? string.Join(",", changed) : null;
        }
        catch { return null; }
    }
}
