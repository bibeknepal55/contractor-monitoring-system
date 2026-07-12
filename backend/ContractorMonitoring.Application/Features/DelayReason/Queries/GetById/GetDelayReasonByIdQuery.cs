using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.DelayReason;

namespace ContractorMonitoring.Application.Features.DelayReason.Queries.GetById;

public record GetDelayReasonByIdQuery : IRequest<ApiResponse<DelayReasonDto>>
{
    public Guid Id { get; init; }
}
