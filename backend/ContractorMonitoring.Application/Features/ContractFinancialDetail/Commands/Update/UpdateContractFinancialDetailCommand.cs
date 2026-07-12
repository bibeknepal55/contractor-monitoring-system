using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ContractFinancialDetail;

namespace ContractorMonitoring.Application.Features.ContractFinancialDetail.Commands.Update;

public record UpdateContractFinancialDetailCommand : IRequest<ApiResponse<ContractFinancialDetailDto>>
{
    public Guid Id { get; init; }
    public UpdateContractFinancialDetailDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
}