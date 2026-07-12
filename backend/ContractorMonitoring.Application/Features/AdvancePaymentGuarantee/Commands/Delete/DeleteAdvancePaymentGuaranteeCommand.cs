using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.AdvancePaymentGuarantee.Commands.Delete;

public record DeleteAdvancePaymentGuaranteeCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; init; }
}
