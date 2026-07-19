using Microsoft.EntityFrameworkCore;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Infrastructure.Data;

namespace ContractorMonitoring.Infrastructure.Services;

// Centralized permission resolution - database-executed queries, not in-memory
public class PermissionResolver : IPermissionResolver
{
    private readonly ApplicationDbContext _context;

    public PermissionResolver(ApplicationDbContext context)
    {
        _context = context;
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
        // Check if SuperAdmin first
        var roles = await GetUserRolesAsync(userId);
        if (roles.Contains("SuperAdmin"))
        {
            return await _context.Permissions
                .Where(p => !p.IsDeleted)
                .Select(p => p.Name)
                .ToListAsync();
        }

        // Get permissions through roles
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId && !ur.IsDeleted)
            .Join(_context.RolePermissions.Where(rp => !rp.IsDeleted),
                ur => ur.RoleId, rp => rp.RoleId,
                (ur, rp) => rp.PermissionId)
            .Join(_context.Permissions.Where(p => !p.IsDeleted),
                permId => permId, p => p.Id,
                (permId, p) => p.Name)
            .Distinct()
            .ToListAsync();
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