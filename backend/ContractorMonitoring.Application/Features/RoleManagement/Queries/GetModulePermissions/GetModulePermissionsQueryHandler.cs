using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RoleManagement;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.RoleManagement.Queries.GetModulePermissions;

public class GetModulePermissionsQueryHandler : IRequestHandler<GetModulePermissionsQuery, ApiResponse<List<ModulePermissionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetModulePermissionsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ApiResponse<List<ModulePermissionDto>>> Handle(GetModulePermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _unitOfWork.Permissions.GetAllAsync();
        var activePermissions = permissions.Where(p => !p.IsDeleted).ToList();

        // Admin cannot see UserManagement and RoleManagement permissions
        if (!request.IsSuperAdmin)
        {
            activePermissions = activePermissions
                .Where(p => p.Group != "UserManagement" && p.Group != "RoleManagement")
                .ToList();
        }

        var modules = activePermissions
            .GroupBy(p => p.Group)
            .Select(g => new ModulePermissionDto
            {
                ModuleName = g.Key,
                ModuleGroup = g.Key,
                Permissions = g.Select(p => new PermissionItemDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category
                }).OrderBy(p => p.Category).ToList()
            })
            .OrderBy(m => m.ModuleName)
            .ToList();

        return ApiResponse<List<ModulePermissionDto>>.Ok(modules, "Module permissions retrieved");
    }
}