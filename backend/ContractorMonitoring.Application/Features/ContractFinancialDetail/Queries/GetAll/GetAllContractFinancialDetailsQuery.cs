using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ContractFinancialDetail;

namespace ContractorMonitoring.Application.Features.ContractFinancialDetail.Queries.GetAll;

public record GetAllContractFinancialDetailsQuery : IRequest<PagedResponse<ContractFinancialDetailDto>>
{
    public PaginationFilter Filter { get; init; } = new();
    public Guid TenantId { get; init; }
}