using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Subcontractor.Commands.Delete;

public class DeleteSubcontractorCommandHandler : IRequestHandler<DeleteSubcontractorCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSubcontractorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteSubcontractorCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Subcontractors.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<bool>.Fail("Subcontractor not found");

        await _unitOfWork.Subcontractors.SoftDeleteAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Subcontractor deleted successfully");
    }
}
