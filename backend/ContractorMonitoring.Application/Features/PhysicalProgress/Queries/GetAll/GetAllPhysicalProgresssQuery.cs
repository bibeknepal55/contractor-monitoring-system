using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhysicalProgress;

namespace ContractorMonitoring.Application.Features.PhysicalProgress.Queries.GetAll;

public record GetAllPhysicalProgressesQuery : IRequest<PagedResponse<PhysicalProgressDto>>
{
    public PaginationFilter Filter { get; init; } = new();
    public Guid TenantId { get; init; }
}

