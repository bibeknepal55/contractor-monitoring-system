using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Project;

namespace ContractorMonitoring.Application.Features.Project.Commands.Create;

// Create project command
public record CreateProjectCommand : IRequest<ApiResponse<ProjectDto>>
{
    public CreateProjectDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
}