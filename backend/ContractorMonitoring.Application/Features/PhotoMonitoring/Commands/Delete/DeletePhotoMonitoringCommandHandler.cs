using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PhotoMonitoring.Commands.Delete;

public class DeletePhotoMonitoringCommandHandler : IRequestHandler<DeletePhotoMonitoringCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePhotoMonitoringCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeletePhotoMonitoringCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.PhotoMonitorings.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<bool>.Fail("PhotoMonitoring not found");

        await _unitOfWork.PhotoMonitorings.SoftDeleteAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "PhotoMonitoring deleted successfully");
    }
}
