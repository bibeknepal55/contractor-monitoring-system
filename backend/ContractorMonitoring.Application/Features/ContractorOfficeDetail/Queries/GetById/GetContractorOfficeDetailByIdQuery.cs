using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ContractorOfficeDetail;

namespace ContractorMonitoring.Application.Features.ContractorOfficeDetail.Queries.GetById;

public record GetContractorOfficeDetailByIdQuery : IRequest<ApiResponse<ContractorOfficeDetailDto>>
{
    public Guid Id { get; init; }
}