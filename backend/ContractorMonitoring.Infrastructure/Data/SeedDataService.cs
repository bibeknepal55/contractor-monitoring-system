using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ContractorMonitoring.Domain.Constants;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Data;

// Database seed service for initial roles, permissions, and admin user
public static class SeedDataService
{
    public static async Task SeedAsync(ApplicationDbContext context, IConfiguration configuration)
    {
        await EnsureRequiredDatabaseColumnsAsync(context);

        // Seed Permissions
        await SeedPermissionsAsync(context);

        // Seed Roles
        await SeedRolesAsync(context);

        // Seed Role-Permission assignments
        await SeedRolePermissionsAsync(context);

        // Seed SuperAdmin user
        await SeedSuperAdminAsync(context, configuration);
    }

    private static async Task EnsureRequiredDatabaseColumnsAsync(ApplicationDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync(@"
            ALTER TABLE ""Roles""
            ADD COLUMN IF NOT EXISTS ""CreatedByUser"" character varying(200),
            ADD COLUMN IF NOT EXISTS ""IsSystem"" boolean NOT NULL DEFAULT false;");

        await context.Database.ExecuteSqlRawAsync(@"
            ALTER TABLE ""Users""
            ADD COLUMN IF NOT EXISTS ""IsApproved"" boolean NOT NULL DEFAULT true,
            ADD COLUMN IF NOT EXISTS ""RefreshTokenFamily"" character varying(500);");

        // Register any out-of-band migrations so EF Core doesn't attempt to re-apply them
        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
            SELECT '20260726000001_AddMissingIndexes', '8.0.0'
            WHERE NOT EXISTS (
                SELECT 1 FROM ""__EFMigrationsHistory"" WHERE ""MigrationId"" = '20260726000001_AddMissingIndexes'
            );");

        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
            SELECT '20260726000002_AddRefreshTokenFamily', '8.0.0'
            WHERE NOT EXISTS (
                SELECT 1 FROM ""__EFMigrationsHistory"" WHERE ""MigrationId"" = '20260726000002_AddRefreshTokenFamily'
            );");
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
        {
            var existingRoles = await context.Roles.ToListAsync();
            if (!existingRoles.Any(r => r.Name == "SuperAdmin"))
            {
                await context.Roles.AddAsync(new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "SuperAdmin",
                    Description = "Super Administrator with full system access",
                    IsSystem = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    TenantId = Guid.Empty
                });
                await context.SaveChangesAsync();
            }
            return;
        }

        var roles = new List<Role>
        {
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "SuperAdmin",
                Description = "Super Administrator with full system access",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                TenantId = Guid.Empty
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                Description = "Administrator with management capabilities",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                TenantId = Guid.Empty
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "Viewer",
                Description = "Read-only access to the system",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                TenantId = Guid.Empty
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Description = "Test role with full CRUD access for testing",
                IsSystem = true,
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

    private static async Task SeedSuperAdminAsync(ApplicationDbContext context, IConfiguration configuration)
    {
        var existingSuperAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == "superadmin@contractor.monitoring");

        var superAdminRole = await context.Roles.FirstAsync(r => r.Name == "SuperAdmin");

        var rawPassword = configuration["Seed:SuperAdminPassword"]
            ?? throw new InvalidOperationException(
                "Seed:SuperAdminPassword is not configured. Set it via environment variable SEED__SUPERADMINPASSWORD or user-secrets.");

        if (existingSuperAdmin == null)
        {
            var superAdmin = new User
            {
                Id = Guid.NewGuid(),
                Email = "superadmin@contractor.monitoring",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword),
                FirstName = "Super",
                LastName = "Admin",
                IsActive = true,
                IsApproved = true,
                MustChangePassword = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                TenantId = Guid.Empty
            };

            await context.Users.AddAsync(superAdmin);
            await context.SaveChangesAsync();

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
            return;
        }

        existingSuperAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);
        existingSuperAdmin.IsActive = true;
        existingSuperAdmin.IsApproved = true;
        existingSuperAdmin.MustChangePassword = true;
        existingSuperAdmin.UpdatedAt = DateTime.UtcNow;
        existingSuperAdmin.UpdatedBy = "System";
        await context.SaveChangesAsync();

        if (!await context.UserRoles.AnyAsync(ur => ur.UserId == existingSuperAdmin.Id && ur.RoleId == superAdminRole.Id))
        {
            await context.UserRoles.AddAsync(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = existingSuperAdmin.Id,
                RoleId = superAdminRole.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                TenantId = Guid.Empty
            });
            await context.SaveChangesAsync();
        }
    }
}