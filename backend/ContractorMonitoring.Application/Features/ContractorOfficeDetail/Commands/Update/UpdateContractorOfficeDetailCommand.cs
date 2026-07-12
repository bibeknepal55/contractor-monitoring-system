using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ContractorOfficeDetail;

namespace ContractorMonitoring.Application.Features.ContractorOfficeDetail.Commands.Update;

public record UpdateContractorOfficeDetailCommand : IRequest<ApiResponse<ContractorOfficeDetailDto>>
{
    public Guid Id { get; init; }
    public UpdateContractorOfficeDetailDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
}