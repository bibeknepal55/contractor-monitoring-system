using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.UserManagement;

namespace ContractorMonitoring.Application.Features.UserManagement.Queries.GetAll;

public record GetAllUsersQuery : IRequest<PagedResponse<UserManagementDto>>
{
    public PaginationFilter Filter { get; init; } = new();
    public Guid TenantId { get; init; }
}