using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhysicalProgress;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PhysicalProgress.Commands.Update;

public class UpdatePhysicalProgressCommandHandler : IRequestHandler<UpdatePhysicalProgressCommand, ApiResponse<PhysicalProgressDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdatePhysicalProgressCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PhysicalProgressDto>> Handle(UpdatePhysicalProgressCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.PhysicalProgresses.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<PhysicalProgressDto>.Fail("PhysicalProgress not found");

        _mapper.Map(command.Request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = command.UserId.ToString();

        await _unitOfWork.PhysicalProgresses.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<PhysicalProgressDto>(entity);
        return ApiResponse<PhysicalProgressDto>.Ok(dto, "PhysicalProgress updated successfully");
    }
}

