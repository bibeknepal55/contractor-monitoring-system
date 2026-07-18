using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.RoleManagement.Commands.DeleteRole;

public record DeleteRoleCommand : IRequest<ApiResponse<bool>>
{
    public Guid RoleId { get; init; }
}