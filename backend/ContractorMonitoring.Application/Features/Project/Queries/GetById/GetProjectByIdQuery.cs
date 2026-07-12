using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Project;

namespace ContractorMonitoring.Application.Features.Project.Queries.GetById;

// Get project by id query
public record GetProjectByIdQuery : IRequest<ApiResponse<ProjectDto>>
{
    public Guid Id { get; init; }
}