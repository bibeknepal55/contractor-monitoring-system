using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RoleManagement;

namespace ContractorMonitoring.Application.Features.RoleManagement.Commands.UpdateRole;

public record UpdateRoleCommand : IRequest<ApiResponse<RoleDto>>
{
    public Guid RoleId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<Guid> PermissionIds { get; init; } = new();
    public bool IsSuperAdmin { get; init; }
}