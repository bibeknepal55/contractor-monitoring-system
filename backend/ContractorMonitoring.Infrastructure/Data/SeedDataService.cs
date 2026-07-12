using Microsoft.EntityFrameworkCore;
using ContractorMonitoring.Domain.Constants;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data;

// Database seed service for initial roles, permissions, and admin user
public static class SeedDataService
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Seed Permissions
        await SeedPermissionsAsync(context);

        // Seed Roles
        await SeedRolesAsync(context);

        // Seed Role-Permission assignments
        await SeedRolePermissionsAsync(context);

        // Seed SuperAdmin user
        await SeedSuperAdminAsync(context);
    }

    private static async Task SeedPermissionsAsync(ApplicationDbContext context)
    {
        if (await context.Permissions.AnyAsync())
            return;

        var allPermissions = Permissions.GetAllPermissions();
        var permissionEntities = new List<Permission>();

        foreach (var permissionName in allPermissions)
        {
            var parts = permissionName.Split('.');
            var group = parts[0]; // Module name
            var category = parts[1]; // CRUD operation

            permissionEntities.Add(new Permission
            {
                Id = Guid.NewGuid(),
                Name = permissionName,
                Description = $"{category} permission for {group}",
                Group = group,
                Category = category,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                TenantId = Guid.Empty
            });
        }

        await context.Permissions.AddRangeAsync(permissionEntities);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context)
    {
        if (await context.Roles.AnyAsync())
            return;

        var roles = new List<Role>
        {
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "SuperAdmin",
                Description = "Super Administrator with full system access",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                TenantId = Guid.Empty
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                Description = "Administrator with management capabilities",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                TenantId = Guid.Empty
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "Viewer",
                Description = "Read-only access to the system",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                TenantId = Guid.Empty
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Description = "Test role with full CRUD access for testing",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                TenantId = Guid.Empty
            }
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolePermissionsAsync(ApplicationDbContext context)
    {
        if (await context.RolePermissions.AnyAsync())
            return;

        var allPermissions = await context.Permissions.ToListAsync();
        var roles = await context.Roles.ToListAsync();

        var superAdminRole = roles.First(r => r.Name == "SuperAdmin");
        var adminRole = roles.First(r => r.Name == "Admin");
        var viewerRole = roles.First(r => r.Name == "Viewer");
        var testRole = roles.First(r => r.Name == "Test");

        var rolePermissions = new List<RolePermission>();

        // SuperAdmin gets ALL permissions
        foreach (var permission in allPermissions)
        {
            rolePermissions.Add(new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = superAdminRole.Id,
                PermissionId = permission.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                TenantId = Guid.Empty
            });
        }

        // Test role gets ALL CRUD permissions for ALL modules
        foreach (var permission in allPermissions)
        {
            rolePermissions.Add(new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = testRole.Id,
                PermissionId = permission.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                TenantId = Guid.Empty
            });
        }

        // Admin gets all permissions except User Management Delete and AssignRole
        foreach (var permission in allPermissions)
        {
            if (permission.Name != Permissions.UserManagement.Delete &&
                permission.Name != Permissions.UserManagement.AssignRole)
            {
                rolePermissions.Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    TenantId = Guid.Empty
                });
            }
        }

        // Viewer gets only View permissions across all modules
        foreach (var permission in allPermissions.Where(p => p.Category == "View"))
        {
            rolePermissions.Add(new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = viewerRole.Id,
                PermissionId = permission.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                TenantId = Guid.Empty
            });
        }

        await context.RolePermissions.AddRangeAsync(rolePermissions);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSuperAdminAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.Email == "superadmin@contractor.monitoring"))
            return;

        var superAdminRole = await context.Roles.FirstAsync(r => r.Name == "SuperAdmin");

        var superAdmin = new User
        {
            Id = Guid.NewGuid(),
            Email = "superadmin@contractor.monitoring",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin@123"),
            FirstName = "Super",
            LastName = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            TenantId = Guid.Empty
        };

        await context.Users.AddAsync(superAdmin);
        await context.SaveChangesAsync();

        // Assign SuperAdmin role
        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = superAdmin.Id,
            RoleId = superAdminRole.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            TenantId = Guid.Empty
        };

        await context.UserRoles.AddAsync(userRole);
        await context.SaveChangesAsync();
    }
}