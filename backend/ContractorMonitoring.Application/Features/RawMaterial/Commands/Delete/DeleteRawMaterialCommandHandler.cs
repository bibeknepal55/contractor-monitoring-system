using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.RawMaterial.Commands.Delete;

public class DeleteRawMaterialCommandHandler : IRequestHandler<DeleteRawMaterialCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRawMaterialCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteRawMaterialCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.RawMaterials.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<bool>.Fail("RawMaterial not found");

        await _unitOfWork.RawMaterials.SoftDeleteAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "RawMaterial deleted successfully");
    }
}
