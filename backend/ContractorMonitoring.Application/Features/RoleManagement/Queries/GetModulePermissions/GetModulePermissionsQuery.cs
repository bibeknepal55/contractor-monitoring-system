using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RoleManagement;

namespace ContractorMonitoring.Application.Features.RoleManagement.Queries.GetModulePermissions;

public record GetModulePermissionsQuery : IRequest<ApiResponse<List<ModulePermissionDto>>>
{
    public bool IsSuperAdmin { get; init; }
}