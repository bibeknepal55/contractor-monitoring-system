using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.DelayReason.Commands.Delete;

public record DeleteDelayReasonCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; init; }
}
