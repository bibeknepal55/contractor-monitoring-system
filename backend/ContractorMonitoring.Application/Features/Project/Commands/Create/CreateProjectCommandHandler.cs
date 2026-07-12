using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Project;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Project.Commands.Create;

// Handler for creating a project
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ApiResponse<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateProjectCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ProjectDto>> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        // Check if contractor exists
        var contractorExists = await _unitOfWork.ContractorOfficeDetails
            .ExistsAsync(c => c.Id == command.Request.ContractorId);

        if (!contractorExists)
        {
            return ApiResponse<ProjectDto>.Fail("Contractor not found");
        }

        // Check if project code already exists
        var projectCodeExists = await _unitOfWork.Projects
            .ExistsAsync(p => p.ProjectCode == command.Request.ProjectCode);

        if (projectCodeExists)
        {
            return ApiResponse<ProjectDto>.Fail("Project code already exists");
        }

        // Map DTO to entity
        var project = _mapper.Map<Domain.Entities.Project>(command.Request);

        // Set audit fields
        project.Id = Guid.NewGuid();
        project.CreatedAt = DateTime.UtcNow;
        project.CreatedBy = command.UserId.ToString();
        project.TenantId = command.TenantId;
        project.ProgressPercentage = 0;
        project.IsDeleted = false;

        // Save to database
        await _unitOfWork.Projects.AddAsync(project);
        await _unitOfWork.SaveChangesAsync();

        // Get contractor name for response
        var contractor = await _unitOfWork.ContractorOfficeDetails.GetByIdAsync(project.ContractorId);
        project.Contractor = contractor!;

        // Map to response DTO
        var projectDto = _mapper.Map<ProjectDto>(project);

        return ApiResponse<ProjectDto>.Ok(projectDto, "Project created successfully");
    }
}