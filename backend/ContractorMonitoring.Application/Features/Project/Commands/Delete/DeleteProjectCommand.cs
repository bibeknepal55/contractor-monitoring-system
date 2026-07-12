using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.Project.Commands.Delete;

// Delete project command (soft delete)
public record DeleteProjectCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; init; }
}