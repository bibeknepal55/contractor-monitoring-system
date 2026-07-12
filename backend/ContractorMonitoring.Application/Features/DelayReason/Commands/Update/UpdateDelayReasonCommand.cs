using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.DelayReason;

namespace ContractorMonitoring.Application.Features.DelayReason.Commands.Update;

public record UpdateDelayReasonCommand : IRequest<ApiResponse<DelayReasonDto>>
{
    public Guid Id { get; init; }
    public UpdateDelayReasonDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
}
