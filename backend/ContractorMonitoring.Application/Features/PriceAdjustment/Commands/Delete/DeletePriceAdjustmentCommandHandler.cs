using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PriceAdjustment.Commands.Delete;

public class DeletePriceAdjustmentCommandHandler : IRequestHandler<DeletePriceAdjustmentCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePriceAdjustmentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeletePriceAdjustmentCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.PriceAdjustments.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<bool>.Fail("PriceAdjustment not found");

        await _unitOfWork.PriceAdjustments.SoftDeleteAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "PriceAdjustment deleted successfully");
    }
}
