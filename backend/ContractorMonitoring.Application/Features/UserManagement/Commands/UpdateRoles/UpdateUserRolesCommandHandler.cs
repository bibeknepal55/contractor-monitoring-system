using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.UserManagement;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Application.Features.UserManagement.Commands.UpdateRoles;

public class UpdateUserRolesCommandHandler : IRequestHandler<UpdateUserRolesCommand, ApiResponse<UserManagementDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserRolesCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<UserManagementDto>> Handle(UpdateUserRolesCommand command, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(command.UserId);
        if (user == null)
            return ApiResponse<UserManagementDto>.Fail("User not found");

        // Get all roles and user roles
        var allUserRoles = await _unitOfWork.UserRoles.GetAllAsync();
        var allRoles = await _unitOfWork.Roles.GetAllAsync();

        // Get updater's roles (the person making the change)
        var updaterRoles = (from ur in allUserRoles
                            join r in allRoles on ur.RoleId equals r.Id
                            where ur.UserId == command.UpdatedBy && !ur.IsDeleted && !r.IsDeleted
                            select r.Name).ToList();

        // Get target user's current roles
        var targetUserRoles = (from ur in allUserRoles
                               join r in allRoles on ur.RoleId equals r.Id
                               where ur.UserId == command.UserId && !ur.IsDeleted && !r.IsDeleted
                               select r.Name).ToList();

        bool isUpdaterSuperAdmin = updaterRoles.Contains("SuperAdmin");
        bool isUpdaterAdmin = updaterRoles.Contains("Admin");
        bool isUpdaterTest = updaterRoles.Contains("Test");

        // System roles that cannot be assigned by non-SuperAdmin
        var systemRoleNames = new HashSet<string> { "SuperAdmin", "Admin" };

        // RBAC: Check role assignment permissions
        foreach (var roleName in command.Request.Roles)
        {
            // SuperAdmin can assign any role
            if (isUpdaterSuperAdmin)
                continue;

            // System roles can only be assigned by SuperAdmin
            if (systemRoleNames.Contains(roleName) && !isUpdaterSuperAdmin)
                return ApiResponse<UserManagementDto>.Fail($"Only SuperAdmin can assign the role: {roleName}");

            // Admin can assign: Test, Viewer, and any custom (non-system) role
            if (isUpdaterAdmin)
            {
                // Check if it's a system role Admin CAN assign
                if (roleName == "Test" || roleName == "Viewer")
                    continue;

                // Check if it's a custom role (not SuperAdmin, not Admin)
                var role = allRoles.FirstOrDefault(r => r.Name == roleName && !r.IsDeleted);
                if (role != null && !role.IsSystem)
                    continue;

                // If we get here, Admin cannot assign this role
                return ApiResponse<UserManagementDto>.Fail($"You do not have permission to assign the role: {roleName}");
            }

            // Test can only assign Test
            if (isUpdaterTest && roleName == "Test")
                continue;

            // Not allowed
            return ApiResponse<UserManagementDto>.Fail($"You do not have permission to assign the role: {roleName}");
        }

        // RBAC: Prevent modifying SuperAdmin users
        if (targetUserRoles.Contains("SuperAdmin") && !isUpdaterSuperAdmin)
            return ApiResponse<UserManagementDto>.Fail("Cannot modify SuperAdmin user");

        // RBAC: Admin cannot modify other Admin users
        if (targetUserRoles.Contains("Admin") && isUpdaterAdmin && command.UserId != command.UpdatedBy)
            return ApiResponse<UserManagementDto>.Fail("Admin cannot modify other Admin users");

        // Multi-tenant: When Viewer is upgraded, move to shared tenant
        bool isCurrentlyOnlyViewer = targetUserRoles.Count == 1 && targetUserRoles.Contains("Viewer");
        bool isBeingUpgraded = command.Request.Roles.Contains("Test") ||
                              command.Request.Roles.Contains("Admin") ||
                              command.Request.Roles.Contains("SuperAdmin") ||
                              command.Request.Roles.Any(r => !systemRoleNames.Contains(r) && r != "Viewer" && r != "Test");

        if (isBeingUpgraded && isCurrentlyOnlyViewer)
        {
            user.TenantId = Guid.Empty;
            await _unitOfWork.Users.UpdateAsync(user);
        }

        // Remove existing roles using soft delete
        var existingRoles = allUserRoles.Where(ur => ur.UserId == command.UserId && !ur.IsDeleted).ToList();
        foreach (var ur in existingRoles)
        {
            ur.IsDeleted = true;
            ur.UpdatedAt = DateTime.UtcNow;
            ur.UpdatedBy = command.UpdatedBy.ToString();
        }
        await _unitOfWork.SaveChangesAsync();

        // Add new roles (reactivate if previously soft-deleted)
        foreach (var roleName in command.Request.Roles)
        {
            var role = allRoles.FirstOrDefault(r => r.Name == roleName && !r.IsDeleted);
            if (role != null)
            {
                var existingDeleted = allUserRoles.FirstOrDefault(ur =>
                    ur.UserId == command.UserId && ur.RoleId == role.Id && ur.IsDeleted);

                if (existingDeleted != null)
                {
                    existingDeleted.IsDeleted = false;
                    existingDeleted.UpdatedAt = DateTime.UtcNow;
                    existingDeleted.UpdatedBy = command.UpdatedBy.ToString();
                }
                else
                {
                    await _unitOfWork.UserRoles.AddAsync(new UserRole
                    {
                        Id = Guid.NewGuid(),
                        UserId = command.UserId,
                        RoleId = role.Id,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = command.UpdatedBy.ToString(),
                        TenantId = user.TenantId,
                        IsDeleted = false
                    });
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();

        // Return updated user
        var roles = await GetUserRoles(command.UserId);
        var permissions = await GetUserPermissions(command.UserId);

        var dto = new UserManagementDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            Roles = roles,
            Permissions = permissions,
            CreatedAt = user.CreatedAt
        };

        return ApiResponse<UserManagementDto>.Ok(dto, "User roles updated successfully");
    }

    private async Task<List<string>> GetUserRoles(Guid userId)
    {
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
        var roles = await _unitOfWork.Roles.GetAllAsync();
        return (from ur in userRoles
                join r in roles on ur.RoleId equals r.Id
                where ur.UserId == userId && !ur.IsDeleted
                select r.Name).ToList();
    }

    private async Task<List<string>> GetUserPermissions(Guid userId)
    {
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
        var rolePermissions = await _unitOfWork.RolePermissions.GetAllAsync();
        var permissions = await _unitOfWork.Permissions.GetAllAsync();
        return (from ur in userRoles
                join rp in rolePermissions on ur.RoleId equals rp.RoleId
                join p in permissions on rp.PermissionId equals p.Id
                where ur.UserId == userId && !ur.IsDeleted && !rp.IsDeleted
                select p.Name).Distinct().ToList();
    }
}