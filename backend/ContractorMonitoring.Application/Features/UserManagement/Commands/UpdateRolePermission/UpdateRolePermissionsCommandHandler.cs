using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Application.Features.UserManagement.Commands.UpdateRolePermissions;

public class UpdateRolePermissionsCommandHandler : IRequestHandler<UpdateRolePermissionsCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRolePermissionsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateRolePermissionsCommand command, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(command.RoleId);
        if (role == null)
            return ApiResponse<bool>.Fail("Role not found");

        // Don't allow modifying SuperAdmin permissions
        if (role.Name == "SuperAdmin")
            return ApiResponse<bool>.Fail("Cannot modify SuperAdmin permissions");

        // Remove existing permissions
        var existingRolePermissions = await _unitOfWork.RolePermissions.GetAllAsync();
        var toRemove = existingRolePermissions.Where(rp => rp.RoleId == command.RoleId).ToList();
        foreach (var rp in toRemove)
            await _unitOfWork.RolePermissions.DeleteAsync(rp);

        // Add new permissions
        var allPermissions = await _unitOfWork.Permissions.GetAllAsync();
        foreach (var permName in command.Permissions)
        {
            var perm = allPermissions.FirstOrDefault(p => p.Name == permName);
            if (perm != null)
            {
                await _unitOfWork.RolePermissions.AddAsync(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = command.RoleId,
                    PermissionId = perm.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    TenantId = role.TenantId
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Role permissions updated successfully");
    }
}