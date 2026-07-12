using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.AdvancePaymentGuarantee;

namespace ContractorMonitoring.Application.Features.AdvancePaymentGuarantee.Commands.Create;

public record CreateAdvancePaymentGuaranteeCommand : IRequest<ApiResponse<AdvancePaymentGuaranteeDto>>
{
    public CreateAdvancePaymentGuaranteeDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
}
