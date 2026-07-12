using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ContractorOfficeDetail.Commands.Delete;

public class DeleteContractorOfficeDetailCommandHandler : IRequestHandler<DeleteContractorOfficeDetailCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteContractorOfficeDetailCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteContractorOfficeDetailCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.ContractorOfficeDetails.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<bool>.Fail("Contractor not found");

        var hasProjects = await _unitOfWork.Projects.ExistsAsync(p => p.ContractorId == command.Id);
        if (hasProjects)
            return ApiResponse<bool>.Fail("Cannot delete contractor with existing projects");

        await _unitOfWork.ContractorOfficeDetails.SoftDeleteAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Contractor deleted successfully");
    }
}