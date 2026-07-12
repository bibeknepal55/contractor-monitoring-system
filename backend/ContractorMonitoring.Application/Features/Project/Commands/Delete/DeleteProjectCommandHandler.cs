using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Project.Commands.Delete;

// Handler for soft deleting a project
public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProjectCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteProjectCommand command, CancellationToken cancellationToken)
    {
        // Get existing project
        var project = await _unitOfWork.Projects.GetByIdAsync(command.Id);

        if (project == null)
        {
            return ApiResponse<bool>.Fail("Project not found");
        }

        // Soft delete
        await _unitOfWork.Projects.SoftDeleteAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Project deleted successfully");
    }
}