using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Project;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Project.Queries.GetById;

// Handler for getting a project by id
public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ApiResponse<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProjectByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        // Get project with contractor details
        var projects = await _unitOfWork.Projects.GetAllAsync();
        var project = projects.FirstOrDefault(p => p.Id == request.Id);

        if (project == null)
        {
            return ApiResponse<ProjectDto>.Fail("Project not found");
        }

        // Get contractor
        var contractor = await _unitOfWork.ContractorOfficeDetails.GetByIdAsync(project.ContractorId);
        project.Contractor = contractor!;

        // Map to DTO
        var projectDto = _mapper.Map<ProjectDto>(project);

        return ApiResponse<ProjectDto>.Ok(projectDto, "Project retrieved successfully");
    }
}