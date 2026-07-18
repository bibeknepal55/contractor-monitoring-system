using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RoleManagement;

namespace ContractorMonitoring.Application.Features.RoleManagement.Queries.GetRoleById;

public record GetRoleByIdQuery : IRequest<ApiResponse<RoleDto>>
{
    public Guid RoleId { get; init; }
}