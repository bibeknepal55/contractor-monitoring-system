using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.DelayReason;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.DelayReason.Commands.Update;

public class UpdateDelayReasonCommandHandler : IRequestHandler<UpdateDelayReasonCommand, ApiResponse<DelayReasonDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateDelayReasonCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<DelayReasonDto>> Handle(UpdateDelayReasonCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.DelayReasons.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<DelayReasonDto>.Fail("DelayReason not found");

        _mapper.Map(command.Request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = command.UserId.ToString();

        await _unitOfWork.DelayReasons.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<DelayReasonDto>(entity);
        return ApiResponse<DelayReasonDto>.Ok(dto, "DelayReason updated successfully");
    }
}
