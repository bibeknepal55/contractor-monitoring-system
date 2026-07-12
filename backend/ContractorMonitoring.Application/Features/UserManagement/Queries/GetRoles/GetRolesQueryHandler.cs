using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.UserManagement;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.UserManagement.Queries.GetRoles;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, ApiResponse<List<RoleManagementDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRolesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<List<RoleManagementDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _unitOfWork.Roles.GetAllAsync();
        var rolePermissions = await _unitOfWork.RolePermissions.GetAllAsync();
        var permissions = await _unitOfWork.Permissions.GetAllAsync();
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync();

        var dtos = roles.Select(r => new RoleManagementDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Permissions = (from rp in rolePermissions
                           join p in permissions on rp.PermissionId equals p.Id
                           where rp.RoleId == r.Id && !rp.IsDeleted
                           select p.Name).ToList(),
            UserCount = userRoles.Count(ur => ur.RoleId == r.Id && !ur.IsDeleted)
        }).ToList();

        return ApiResponse<List<RoleManagementDto>>.Ok(dtos, "Roles retrieved successfully");
    }
}