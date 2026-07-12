using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.PriceAdjustment.Commands.Delete;

public record DeletePriceAdjustmentCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; init; }
}
