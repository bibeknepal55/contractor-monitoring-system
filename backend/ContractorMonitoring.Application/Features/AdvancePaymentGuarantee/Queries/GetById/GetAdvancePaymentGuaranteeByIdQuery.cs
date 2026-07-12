using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.AdvancePaymentGuarantee;

namespace ContractorMonitoring.Application.Features.AdvancePaymentGuarantee.Queries.GetById;

public record GetAdvancePaymentGuaranteeByIdQuery : IRequest<ApiResponse<AdvancePaymentGuaranteeDto>>
{
    public Guid Id { get; init; }
}
