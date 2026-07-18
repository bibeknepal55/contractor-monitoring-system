using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RoleManagement;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.RoleManagement.Queries.GetRoleById;

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, ApiResponse<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRoleByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ApiResponse<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId);
        if (role == null || role.IsDeleted)
            return ApiResponse<RoleDto>.Fail("Role not found");

        var rolePermissions = await _unitOfWork.RolePermissions.GetAllAsync();
        var permissions = await _unitOfWork.Permissions.GetAllAsync();
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync();

        var dto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystem = role.IsSystem,
            UserCount = userRoles.Count(ur => ur.RoleId == role.Id && !ur.IsDeleted),
            Permissions = (from rp in rolePermissions
                           join p in permissions on rp.PermissionId equals p.Id
                           where rp.RoleId == role.Id && !rp.IsDeleted
                           select p.Name).ToList(),
            CreatedAt = role.CreatedAt
        };

        return ApiResponse<RoleDto>.Ok(dto, "Role retrieved successfully");
    }
}