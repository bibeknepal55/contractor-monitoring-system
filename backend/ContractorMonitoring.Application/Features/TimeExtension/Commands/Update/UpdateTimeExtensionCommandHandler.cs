using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.TimeExtension;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.TimeExtension.Commands.Update;

public class UpdateTimeExtensionCommandHandler : IRequestHandler<UpdateTimeExtensionCommand, ApiResponse<TimeExtensionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateTimeExtensionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<TimeExtensionDto>> Handle(UpdateTimeExtensionCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.TimeExtensions.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<TimeExtensionDto>.Fail("TimeExtension not found");

        _mapper.Map(command.Request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = command.UserId.ToString();

        await _unitOfWork.TimeExtensions.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<TimeExtensionDto>(entity);
        return ApiResponse<TimeExtensionDto>.Ok(dto, "TimeExtension updated successfully");
    }
}
