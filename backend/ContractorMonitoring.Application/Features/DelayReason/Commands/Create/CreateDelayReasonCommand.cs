using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.DelayReason;

namespace ContractorMonitoring.Application.Features.DelayReason.Commands.Create;

public record CreateDelayReasonCommand : IRequest<ApiResponse<DelayReasonDto>>
{
    public CreateDelayReasonDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
}
