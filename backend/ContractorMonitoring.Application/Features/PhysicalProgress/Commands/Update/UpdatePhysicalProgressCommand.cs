using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhysicalProgress;

namespace ContractorMonitoring.Application.Features.PhysicalProgress.Commands.Update;

public record UpdatePhysicalProgressCommand : IRequest<ApiResponse<PhysicalProgressDto>>
{
    public Guid Id { get; init; }
    public UpdatePhysicalProgressDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
}

