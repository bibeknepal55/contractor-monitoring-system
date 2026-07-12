using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.DelayReason;

namespace ContractorMonitoring.Application.Features.DelayReason.Queries.GetAll;

public record GetAllDelayReasonsQuery : IRequest<PagedResponse<DelayReasonDto>>
{
    public PaginationFilter Filter { get; init; } = new();
    public Guid TenantId { get; init; }
}
