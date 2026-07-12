using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ContractorOfficeDetail;

namespace ContractorMonitoring.Application.Features.ContractorOfficeDetail.Queries.GetAll;

public record GetAllContractorOfficeDetailsQuery : IRequest<PagedResponse<ContractorOfficeDetailDto>>
{
    public PaginationFilter Filter { get; init; } = new();
    public Guid TenantId { get; init; }
}