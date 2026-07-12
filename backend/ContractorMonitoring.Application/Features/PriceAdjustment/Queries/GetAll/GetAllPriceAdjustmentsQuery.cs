using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PriceAdjustment;

namespace ContractorMonitoring.Application.Features.PriceAdjustment.Queries.GetAll;

public record GetAllPriceAdjustmentsQuery : IRequest<PagedResponse<PriceAdjustmentDto>>
{
    public PaginationFilter Filter { get; init; } = new();
    public Guid TenantId { get; init; }
}
