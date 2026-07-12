using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ContractFinancialDetail;

namespace ContractorMonitoring.Application.Features.ContractFinancialDetail.Queries.GetById;

public record GetContractFinancialDetailByIdQuery : IRequest<ApiResponse<ContractFinancialDetailDto>>
{
    public Guid Id { get; init; }
}