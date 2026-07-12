using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.AdvancePaymentGuarantee.Commands.Delete;

public class DeleteAdvancePaymentGuaranteeCommandHandler : IRequestHandler<DeleteAdvancePaymentGuaranteeCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAdvancePaymentGuaranteeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteAdvancePaymentGuaranteeCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.AdvancePaymentGuarantees.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<bool>.Fail("AdvancePaymentGuarantee not found");

        await _unitOfWork.AdvancePaymentGuarantees.SoftDeleteAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "AdvancePaymentGuarantee deleted successfully");
    }
}
