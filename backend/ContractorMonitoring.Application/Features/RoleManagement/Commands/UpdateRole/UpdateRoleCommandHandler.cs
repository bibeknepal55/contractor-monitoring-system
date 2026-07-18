using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RoleManagement;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Application.Features.RoleManagement.Commands.UpdateRole;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, ApiResponse<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoleCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ApiResponse<RoleDto>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId);
        if (role == null || role.IsDeleted)
            return ApiResponse<RoleDto>.Fail("Role not found");

        // Check if new name conflicts with existing role
        var allRoles = await _unitOfWork.Roles.GetAllAsync();
        if (allRoles.Any(r => r.Name == request.Name && r.Id != request.RoleId && !r.IsDeleted))
            return ApiResponse<RoleDto>.Fail("Role name already exists");

        // Admin cannot grant UserManagement permissions
        if (!request.IsSuperAdmin)
        {
            var allPermissions = await _unitOfWork.Permissions.GetAllAsync();
            var restrictedPermIds = allPermissions.Where(p => p.Group == "UserManagement").Select(p => p.Id).ToList();
            if (request.PermissionIds.Any(pid => restrictedPermIds.Contains(pid)))
                return ApiResponse<RoleDto>.Fail("Admin cannot grant User Management permissions");
        }

        // Update role details
        role.Name = request.Name;
        role.Description = request.Description;
        role.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Roles.UpdateAsync(role);

        // Remove existing permissions
        var existingPermissions = await _unitOfWork.RolePermissions.GetAllAsync();
        var toRemove = existingPermissions.Where(rp => rp.RoleId == request.RoleId).ToList();
        foreach (var rp in toRemove)
            await _unitOfWork.RolePermissions.DeleteAsync(rp);

        // Add new permissions
        foreach (var permId in request.PermissionIds)
        {
            await _unitOfWork.RolePermissions.AddAsync(new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = request.RoleId,
                PermissionId = permId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system",
                TenantId = role.TenantId,
                IsDeleted = false
            });
        }

        await _unitOfWork.SaveChangesAsync();

        var permissions = await _unitOfWork.Permissions.GetAllAsync();
        var dto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystem = role.IsSystem,
            UserCount = (await _unitOfWork.UserRoles.GetAllAsync()).Count(ur => ur.RoleId == role.Id && !ur.IsDeleted),
            Permissions = permissions.Where(p => request.PermissionIds.Contains(p.Id)).Select(p => p.Name).ToList(),
            CreatedAt = role.CreatedAt
        };

        return ApiResponse<RoleDto>.Ok(dto, "Role updated successfully");
    }
}