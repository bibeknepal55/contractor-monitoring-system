using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ContractorOfficeDetail;

namespace ContractorMonitoring.Application.Features.ContractorOfficeDetail.Commands.Create;

public record CreateContractorOfficeDetailCommand : IRequest<ApiResponse<ContractorOfficeDetailDto>>
{
    public CreateContractorOfficeDetailDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
}