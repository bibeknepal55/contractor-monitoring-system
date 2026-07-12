using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhysicalProgress;

namespace ContractorMonitoring.Application.Features.PhysicalProgress.Queries.GetById;

public record GetPhysicalProgressByIdQuery : IRequest<ApiResponse<PhysicalProgressDto>>
{
    public Guid Id { get; init; }
}

