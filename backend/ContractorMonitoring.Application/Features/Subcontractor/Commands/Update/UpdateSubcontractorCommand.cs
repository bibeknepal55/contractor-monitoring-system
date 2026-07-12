using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Subcontractor;

namespace ContractorMonitoring.Application.Features.Subcontractor.Commands.Update;

public record UpdateSubcontractorCommand : IRequest<ApiResponse<SubcontractorDto>>
{
    public Guid Id { get; init; }
    public UpdateSubcontractorDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
}
