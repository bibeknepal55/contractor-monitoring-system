using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Subcontractor;

namespace ContractorMonitoring.Application.Features.Subcontractor.Commands.Create;

public record CreateSubcontractorCommand : IRequest<ApiResponse<SubcontractorDto>>
{
    public CreateSubcontractorDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
}
