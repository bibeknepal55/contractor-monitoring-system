using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PriceAdjustment;

namespace ContractorMonitoring.Application.Features.PriceAdjustment.Queries.GetById;

public record GetPriceAdjustmentByIdQuery : IRequest<ApiResponse<PriceAdjustmentDto>>
{
    public Guid Id { get; init; }
}
