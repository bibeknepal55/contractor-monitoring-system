using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PerformanceBond;

namespace ContractorMonitoring.Application.Features.PerformanceBond.Commands.Update;

public record UpdatePerformanceBondCommand : IRequest<ApiResponse<PerformanceBondDto>>
{
    public Guid Id { get; init; }
    public UpdatePerformanceBondDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
}
