using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PhysicalProgress.Commands.Delete;

public class DeletePhysicalProgressCommandHandler : IRequestHandler<DeletePhysicalProgressCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePhysicalProgressCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeletePhysicalProgressCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.PhysicalProgresses.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<bool>.Fail("PhysicalProgress not found");

        await _unitOfWork.PhysicalProgresses.SoftDeleteAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "PhysicalProgress deleted successfully");
    }
}

