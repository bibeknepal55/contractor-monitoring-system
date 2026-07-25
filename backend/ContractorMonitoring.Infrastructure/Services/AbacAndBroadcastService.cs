using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Infrastructure.Data;

namespace ContractorMonitoring.Infrastructure.Services;

public class AbacService : IAbacService
{
    private readonly ApplicationDbContext _context;
    private readonly IPermissionResolver _permissionResolver;

    public AbacService(ApplicationDbContext context, IPermissionResolver permissionResolver)
    {
        _context = context;
        _permissionResolver = permissionResolver;
    }

    public async Task<bool> EvaluatePolicyAsync(Guid userId, string resource, string action, Dictionary<string, string> attributes)
    {
        var roles = await _permissionResolver.GetUserRolesAsync(userId);
        if (roles.Contains("SuperAdmin")) return true;

        var policies = await _context.ResourcePolicies
            .Where(p => p.Resource == resource && p.Action == action && !p.IsDeleted)
            .Join(_context.UserRoles.Where(ur => ur.UserId == userId && !ur.IsDeleted),
                p => p.RoleId, ur => ur.RoleId, (p, ur) => p)
            .ToListAsync();

        if (!policies.Any()) return true;

        foreach (var policy in policies)
        {
            if (!attributes.TryGetValue(policy.Attribute, out var attrValue)) continue;
            var matches = policy.Operator switch
            {
                "Equals"    => string.Equals(attrValue, policy.Value, StringComparison.OrdinalIgnoreCase),
                "In"        => policy.Value.Split(',').Any(v => string.Equals(v.Trim(), attrValue, StringComparison.OrdinalIgnoreCase)),
                "StartsWith"=> attrValue.StartsWith(policy.Value, StringComparison.OrdinalIgnoreCase),
                "NotEquals" => !string.Equals(attrValue, policy.Value, StringComparison.OrdinalIgnoreCase),
                _           => false
            };
            if (matches) return true;
        }
        return false;
    }
}

public class PermissionHub : Hub
{
    public async Task JoinUserGroup(string userId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
}

public class PermissionBroadcastService : IPermissionBroadcastService
{
    private readonly IHubContext<PermissionHub> _hub;
    private readonly IPermissionCacheService _cache;

    public PermissionBroadcastService(IHubContext<PermissionHub> hub, IPermissionCacheService cache)
    {
        _hub = hub;
        _cache = cache;
    }

    public async Task BroadcastPermissionChangeAsync(Guid userId)
    {
        await _cache.InvalidateAsync(userId);
        await _hub.Clients.Group($"user_{userId}").SendAsync("PermissionsChanged", userId);
    }

    public async Task BroadcastRoleChangeAsync(Guid roleId)
    {
        await _cache.InvalidateAllAsync();
        await _hub.Clients.All.SendAsync("RoleChanged", roleId);
    }
}
