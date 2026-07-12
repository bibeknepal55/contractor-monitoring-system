using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PerformanceBond.Commands.Delete;

public class DeletePerformanceBondCommandHandler : IRequestHandler<DeletePerformanceBondCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePerformanceBondCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeletePerformanceBondCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.PerformanceBonds.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<bool>.Fail("PerformanceBond not found");

        await _unitOfWork.PerformanceBonds.SoftDeleteAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "PerformanceBond deleted successfully");
    }
}
