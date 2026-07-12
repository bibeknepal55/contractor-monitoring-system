using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.ContractFinancialDetail.Commands.Delete;

public record DeleteContractFinancialDetailCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; init; }
}