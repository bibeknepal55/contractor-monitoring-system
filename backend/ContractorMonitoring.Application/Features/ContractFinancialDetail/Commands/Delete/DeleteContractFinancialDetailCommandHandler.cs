using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ContractFinancialDetail.Commands.Delete;

public class DeleteContractFinancialDetailCommandHandler : IRequestHandler<DeleteContractFinancialDetailCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteContractFinancialDetailCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteContractFinancialDetailCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.ContractFinancialDetails.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<bool>.Fail("Contract financial detail not found");

        await _unitOfWork.ContractFinancialDetails.SoftDeleteAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Contract financial detail deleted successfully");
    }
}