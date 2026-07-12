using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.UserManagement.Commands.UpdateRolePermissions;

public record UpdateRolePermissionsCommand : IRequest<ApiResponse<bool>>
{
    public Guid RoleId { get; init; }
    public List<string> Permissions { get; init; } = new();
}