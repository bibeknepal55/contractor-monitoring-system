using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.PhysicalProgress.Commands.Delete;

public record DeletePhysicalProgressCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; init; }
}

