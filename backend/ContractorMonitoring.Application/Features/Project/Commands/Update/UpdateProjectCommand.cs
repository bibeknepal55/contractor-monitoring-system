using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Project;

namespace ContractorMonitoring.Application.Features.Project.Commands.Update;

// Update project command
public record UpdateProjectCommand : IRequest<ApiResponse<ProjectDto>>
{
    public Guid Id { get; init; }
    public UpdateProjectDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
}