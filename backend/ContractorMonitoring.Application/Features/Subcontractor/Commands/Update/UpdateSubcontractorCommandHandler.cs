using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Subcontractor;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Subcontractor.Commands.Update;

public class UpdateSubcontractorCommandHandler : IRequestHandler<UpdateSubcontractorCommand, ApiResponse<SubcontractorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateSubcontractorCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<SubcontractorDto>> Handle(UpdateSubcontractorCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Subcontractors.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<SubcontractorDto>.Fail("Subcontractor not found");

        _mapper.Map(command.Request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = command.UserId.ToString();

        await _unitOfWork.Subcontractors.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<SubcontractorDto>(entity);
        return ApiResponse<SubcontractorDto>.Ok(dto, "Subcontractor updated successfully");
    }
}
