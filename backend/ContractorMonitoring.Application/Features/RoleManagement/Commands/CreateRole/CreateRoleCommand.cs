using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RoleManagement;

namespace ContractorMonitoring.Application.Features.RoleManagement.Commands.CreateRole;

public record CreateRoleCommand : IRequest<ApiResponse<RoleDto>>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<Guid> PermissionIds { get; init; } = new();
    public string CreatedBy { get; init; } = string.Empty;
    public bool IsSuperAdmin { get; init; }
}