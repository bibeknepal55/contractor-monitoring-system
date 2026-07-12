using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Project;

namespace ContractorMonitoring.Application.Features.Project.Queries.GetAll;

// Get all projects query with pagination, filtering, sorting, search
public record GetAllProjectsQuery : IRequest<PagedResponse<ProjectDto>>
{
    public PaginationFilter Filter { get; init; } = new();
    public Guid TenantId { get; init; }
}