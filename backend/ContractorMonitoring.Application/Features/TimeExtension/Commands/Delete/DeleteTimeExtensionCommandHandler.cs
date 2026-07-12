using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.TimeExtension.Commands.Delete;

public class DeleteTimeExtensionCommandHandler : IRequestHandler<DeleteTimeExtensionCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTimeExtensionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteTimeExtensionCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.TimeExtensions.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<bool>.Fail("TimeExtension not found");

        await _unitOfWork.TimeExtensions.SoftDeleteAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "TimeExtension deleted successfully");
    }
}
