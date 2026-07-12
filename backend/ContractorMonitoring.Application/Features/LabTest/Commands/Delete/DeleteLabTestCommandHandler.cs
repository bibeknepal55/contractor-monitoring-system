using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.LabTest.Commands.Delete;

public class DeleteLabTestCommandHandler : IRequestHandler<DeleteLabTestCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteLabTestCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteLabTestCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.LabTests.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<bool>.Fail("LabTest not found");

        await _unitOfWork.LabTests.SoftDeleteAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "LabTest deleted successfully");
    }
}
