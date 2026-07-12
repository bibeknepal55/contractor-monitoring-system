using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.UserManagement;

namespace ContractorMonitoring.Application.Features.UserManagement.Commands.UpdateRoles;

public record UpdateUserRolesCommand : IRequest<ApiResponse<UserManagementDto>>
{
    public Guid UserId { get; init; }
    public UpdateUserRolesDto Request { get; init; } = null!;
    public Guid UpdatedBy { get; init; }
}