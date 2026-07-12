using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.AdvancePaymentGuarantee;

namespace ContractorMonitoring.Application.Features.AdvancePaymentGuarantee.Commands.Update;

public record UpdateAdvancePaymentGuaranteeCommand : IRequest<ApiResponse<AdvancePaymentGuaranteeDto>>
{
    public Guid Id { get; init; }
    public UpdateAdvancePaymentGuaranteeDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
}
