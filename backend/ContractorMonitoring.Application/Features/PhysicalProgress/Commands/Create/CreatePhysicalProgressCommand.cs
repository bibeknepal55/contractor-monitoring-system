using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhysicalProgress;

namespace ContractorMonitoring.Application.Features.PhysicalProgress.Commands.Create;

public record CreatePhysicalProgressCommand : IRequest<ApiResponse<PhysicalProgressDto>>
{
    public CreatePhysicalProgressDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
}

