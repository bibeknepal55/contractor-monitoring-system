using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Project;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Project.Commands.Update;

// Handler for updating a project
public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ApiResponse<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProjectCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ProjectDto>> Handle(UpdateProjectCommand command, CancellationToken cancellationToken)
    {
        // Get existing project
        var project = await _unitOfWork.Projects.GetByIdAsync(command.Id);

        if (project == null)
        {
            return ApiResponse<ProjectDto>.Fail("Project not found");
        }

        // Check if contractor exists
        var contractorExists = await _unitOfWork.ContractorOfficeDetails
            .ExistsAsync(c => c.Id == command.Request.ContractorId);

        if (!contractorExists)
        {
            return ApiResponse<ProjectDto>.Fail("Contractor not found");
        }

        // Map updated fields
        _mapper.Map(command.Request, project);

        // Update audit fields
        project.UpdatedAt = DateTime.UtcNow;
        project.UpdatedBy = command.UserId.ToString();

        // Save changes
        await _unitOfWork.Projects.UpdateAsync(project);
        await _unitOfWork.SaveChangesAsync();

        // Get contractor name for response
        var contractor = await _unitOfWork.ContractorOfficeDetails.GetByIdAsync(project.ContractorId);
        project.Contractor = contractor!;

        // Map to response DTO
        var projectDto = _mapper.Map<ProjectDto>(project);

        return ApiResponse<ProjectDto>.Ok(projectDto, "Project updated successfully");
    }
}