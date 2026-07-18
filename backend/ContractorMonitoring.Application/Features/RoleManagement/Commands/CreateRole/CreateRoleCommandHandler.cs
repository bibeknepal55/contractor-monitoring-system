using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RoleManagement;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Application.Features.RoleManagement.Commands.CreateRole;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, ApiResponse<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ApiResponse<RoleDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // Check if role name already exists
        var allRoles = await _unitOfWork.Roles.GetAllAsync();
        if (allRoles.Any(r => r.Name == request.Name && !r.IsDeleted))
            return ApiResponse<RoleDto>.Fail("Role name already exists");

        // Admin cannot grant UserManagement permissions
        if (!request.IsSuperAdmin)
        {
            var allPermissions = await _unitOfWork.Permissions.GetAllAsync();
            var restrictedPermIds = allPermissions
                .Where(p => p.Group == "UserManagement")
                .Select(p => p.Id)
                .ToList();

            if (request.PermissionIds.Any(pid => restrictedPermIds.Contains(pid)))
                return ApiResponse<RoleDto>.Fail("Admin cannot grant User Management permissions");
        }

        // Create the role
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsSystem = false,
            CreatedByUser = request.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            TenantId = Guid.Empty,
            IsDeleted = false
        };

        await _unitOfWork.Roles.AddAsync(role);

        // Add role permissions
        foreach (var permId in request.PermissionIds)
        {
            await _unitOfWork.RolePermissions.AddAsync(new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = role.Id,
                PermissionId = permId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system",
                TenantId = Guid.Empty,
                IsDeleted = false
            });
        }

        await _unitOfWork.SaveChangesAsync();

        var dto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystem = false,
            UserCount = 0,
            Permissions = (await _unitOfWork.Permissions.GetAllAsync())
                .Where(p => request.PermissionIds.Contains(p.Id))
                .Select(p => p.Name).ToList(),
            CreatedAt = role.CreatedAt
        };

        return ApiResponse<RoleDto>.Ok(dto, "Role created successfully");
    }
}