using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.LabTest;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.LabTest.Commands.Update;

public class UpdateLabTestCommandHandler : IRequestHandler<UpdateLabTestCommand, ApiResponse<LabTestDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateLabTestCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<LabTestDto>> Handle(UpdateLabTestCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.LabTests.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<LabTestDto>.Fail("LabTest not found");

        _mapper.Map(command.Request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = command.UserId.ToString();

        await _unitOfWork.LabTests.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<LabTestDto>(entity);
        return ApiResponse<LabTestDto>.Ok(dto, "LabTest updated successfully");
    }
}
