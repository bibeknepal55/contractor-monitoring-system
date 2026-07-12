using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PriceAdjustment;

namespace ContractorMonitoring.Application.Features.PriceAdjustment.Commands.Update;

public record UpdatePriceAdjustmentCommand : IRequest<ApiResponse<PriceAdjustmentDto>>
{
    public Guid Id { get; init; }
    public UpdatePriceAdjustmentDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
}