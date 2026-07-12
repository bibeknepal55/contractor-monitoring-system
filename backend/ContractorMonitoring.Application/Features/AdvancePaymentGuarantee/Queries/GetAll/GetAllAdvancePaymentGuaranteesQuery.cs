using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.AdvancePaymentGuarantee;

namespace ContractorMonitoring.Application.Features.AdvancePaymentGuarantee.Queries.GetAll;

public record GetAllAdvancePaymentGuaranteesQuery : IRequest<PagedResponse<AdvancePaymentGuaranteeDto>>
{
    public PaginationFilter Filter { get; init; } = new();
    public Guid TenantId { get; init; }
}
