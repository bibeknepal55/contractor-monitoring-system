using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RoleManagement;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.RoleManagement.Queries.GetAllRoles;

public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, ApiResponse<List<RoleDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllRolesQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ApiResponse<List<RoleDto>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _unitOfWork.Roles.GetAllAsync();
        var rolePermissions = await _unitOfWork.RolePermissions.GetAllAsync();
        var permissions = await _unitOfWork.Permissions.GetAllAsync();
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync();

        var dtos = roles.Where(r => !r.IsDeleted).Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            IsSystem = r.IsSystem,
            UserCount = userRoles.Count(ur => ur.RoleId == r.Id && !ur.IsDeleted),
            Permissions = (from rp in rolePermissions
                           join p in permissions on rp.PermissionId equals p.Id
                           where rp.RoleId == r.Id && !rp.IsDeleted
                           select p.Name).ToList(),
            CreatedAt = r.CreatedAt
        }).OrderByDescending(r => r.IsSystem).ThenBy(r => r.Name).ToList();

        return ApiResponse<List<RoleDto>>.Ok(dtos, "Roles retrieved successfully");
    }
}