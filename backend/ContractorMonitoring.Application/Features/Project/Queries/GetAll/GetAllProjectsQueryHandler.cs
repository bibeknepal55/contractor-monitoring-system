using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Project;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Project.Queries.GetAll;

// Handler for getting all projects
public class GetAllProjectsQueryHandler : IRequestHandler<GetAllProjectsQuery, PagedResponse<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllProjectsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<ProjectDto>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
    {
        // Get paged projects filtered by tenant
        var pagedProjects = await _unitOfWork.Projects.GetPagedAsync(
            request.Filter,
            predicate: p => p.TenantId == request.TenantId,
            includes: p => p.Contractor
        );

        // Map to DTOs
        var projectDtos = _mapper.Map<List<ProjectDto>>(pagedProjects.Data);

        // Create paged response
        var pagedResponse = new PagedResponse<ProjectDto>
        {
            Data = projectDtos,
            Page = pagedProjects.Page,
            PageSize = pagedProjects.PageSize,
            TotalCount = pagedProjects.TotalCount,
            TotalPages = pagedProjects.TotalPages,
            Message = "Projects retrieved successfully"
        };

        return pagedResponse;
    }
}