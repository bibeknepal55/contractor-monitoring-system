using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.Subcontractor.Commands.Delete;

public record DeleteSubcontractorCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; init; }
}
