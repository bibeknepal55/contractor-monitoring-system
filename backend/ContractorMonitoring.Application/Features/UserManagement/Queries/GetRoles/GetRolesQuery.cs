using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.UserManagement;

namespace ContractorMonitoring.Application.Features.UserManagement.Queries.GetRoles;

public record GetRolesQuery : IRequest<ApiResponse<List<RoleManagementDto>>>
{
    public Guid TenantId { get; init; }
}