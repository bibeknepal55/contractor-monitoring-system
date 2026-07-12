using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ContractFinancialDetail;

namespace ContractorMonitoring.Application.Features.ContractFinancialDetail.Commands.Create;

public record CreateContractFinancialDetailCommand : IRequest<ApiResponse<ContractFinancialDetailDto>>
{
    public CreateContractFinancialDetailDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
}