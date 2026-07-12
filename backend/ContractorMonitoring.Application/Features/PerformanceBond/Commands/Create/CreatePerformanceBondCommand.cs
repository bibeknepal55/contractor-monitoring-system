using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PerformanceBond;

namespace ContractorMonitoring.Application.Features.PerformanceBond.Commands.Create;

public record CreatePerformanceBondCommand : IRequest<ApiResponse<PerformanceBondDto>>
{
    public CreatePerformanceBondDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
}
