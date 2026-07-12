using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.DelayReason.Commands.Delete;

public class DeleteDelayReasonCommandHandler : IRequestHandler<DeleteDelayReasonCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDelayReasonCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteDelayReasonCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.DelayReasons.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<bool>.Fail("DelayReason not found");

        await _unitOfWork.DelayReasons.SoftDeleteAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "DelayReason deleted successfully");
    }
}
