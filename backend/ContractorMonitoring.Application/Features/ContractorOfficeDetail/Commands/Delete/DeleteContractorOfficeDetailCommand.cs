using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.ContractorOfficeDetail.Commands.Delete;

public record DeleteContractorOfficeDetailCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; init; }
}