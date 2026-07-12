using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PerformanceBond;

namespace ContractorMonitoring.Application.Features.PerformanceBond.Queries.GetAll;

public record GetAllPerformanceBondsQuery : IRequest<PagedResponse<PerformanceBondDto>>
{
    public PaginationFilter Filter { get; init; } = new();
    public Guid TenantId { get; init; }
}
