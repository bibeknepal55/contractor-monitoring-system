using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PriceAdjustment;

namespace ContractorMonitoring.Application.Features.PriceAdjustment.Commands.Create;

public record CreatePriceAdjustmentCommand : IRequest<ApiResponse<PriceAdjustmentDto>>
{
    public CreatePriceAdjustmentDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
    public string UserName { get; init; } = string.Empty;
}