using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ResponsibleOfficial.Commands.Delete;

public class DeleteResponsibleOfficialCommandHandler : IRequestHandler<DeleteResponsibleOfficialCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteResponsibleOfficialCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteResponsibleOfficialCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.ResponsibleOfficials.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<bool>.Fail("ResponsibleOfficial not found");

        await _unitOfWork.ResponsibleOfficials.SoftDeleteAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "ResponsibleOfficial deleted successfully");
    }
}
