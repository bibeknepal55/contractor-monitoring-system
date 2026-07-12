using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.PerformanceBond.Commands.Delete;

public record DeletePerformanceBondCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; init; }
}
