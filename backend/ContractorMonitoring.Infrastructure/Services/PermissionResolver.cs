using Microsoft.EntityFrameworkCore;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Infrastructure.Data;

namespace ContractorMonitoring.Infrastructure.Services;

// Centralized permission resolution with Redis cache (Phase 2)
public class PermissionResolver : IPermissionResolver
{
    private readonly ApplicationDbContext _context;
    private readonly IPermissionCacheService _cache;

    public PermissionResolver(ApplicationDbContext context, IPermissionCacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId && !ur.IsDeleted)
            .Join(_context.Roles.Where(r => !r.IsDeleted),
                ur => ur.RoleId, r => r.Id,
                (ur, r) => r.Name)
            .ToListAsync();
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        // Try Redis cache first
        var cached = await _cache.GetCachedPermissionsAsync(userId);
        if (cached != null) return cached;

        var roles = await GetUserRolesAsync(userId);
        List<string> permissions;

        if (roles.Contains("SuperAdmin"))
        {
            permissions = await _context.Permissions
                .Where(p => !p.IsDeleted)
                .Select(p => p.Name)
                .ToListAsync();
        }
        else
        {
            // Direct role permissions
            var direct = await _context.UserRoles
                .Where(ur => ur.UserId == userId && !ur.IsDeleted)
                .Join(_context.RolePermissions.Where(rp => !rp.IsDeleted),
                    ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp.PermissionId)
                .Join(_context.Permissions.Where(p => !p.IsDeleted),
                    permId => permId, p => p.Id, (permId, p) => p.Name)
                .Distinct()
                .ToListAsync();

            // Phase 3: Inherited permissions via RoleInheritance
            var userRoleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId && !ur.IsDeleted)
                .Select(ur => ur.RoleId).ToListAsync();

            var inheritedRoleIds = await _context.RoleInheritances
                .Where(ri => userRoleIds.Contains(ri.ChildRoleId) && !ri.IsDeleted)
                .Select(ri => ri.ParentRoleId).ToListAsync();

            var inherited = inheritedRoleIds.Any()
                ? await _context.RolePermissions
                    .Where(rp => inheritedRoleIds.Contains(rp.RoleId) && !rp.IsDeleted)
                    .Join(_context.Permissions.Where(p => !p.IsDeleted),
                        rp => rp.PermissionId, p => p.Id, (rp, p) => p.Name)
                    .Distinct().ToListAsync()
                : new List<string>();

            // Phase 3: Time-bound roles
            var timeBoundPerms = await _context.TimeBoundUserRoles
                .Where(tb => tb.UserId == userId && !tb.IsDeleted
                    && tb.ValidFrom <= DateTime.UtcNow && tb.ValidTo >= DateTime.UtcNow)
                .Join(_context.RolePermissions.Where(rp => !rp.IsDeleted),
                    tb => tb.RoleId, rp => rp.RoleId, (tb, rp) => rp.PermissionId)
                .Join(_context.Permissions.Where(p => !p.IsDeleted),
                    permId => permId, p => p.Id, (permId, p) => p.Name)
                .Distinct().ToListAsync();

            permissions = direct.Union(inherited).Union(timeBoundPerms).Distinct().ToList();
        }

        await _cache.SetCachedPermissionsAsync(userId, permissions);
        return permissions;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permission)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Contains(permission);
    }

    public async Task<bool> HasRoleAsync(Guid userId, string role)
    {
        var roles = await GetUserRolesAsync(userId);
        return roles.Contains(role);
    }
}