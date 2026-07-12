using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhotoMonitoring;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PhotoMonitoring.Commands.Update;

public class UpdatePhotoMonitoringCommandHandler : IRequestHandler<UpdatePhotoMonitoringCommand, ApiResponse<PhotoMonitoringDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdatePhotoMonitoringCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PhotoMonitoringDto>> Handle(UpdatePhotoMonitoringCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.PhotoMonitorings.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<PhotoMonitoringDto>.Fail("PhotoMonitoring not found");

        _mapper.Map(command.Request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = command.UserId.ToString();

        await _unitOfWork.PhotoMonitorings.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<PhotoMonitoringDto>(entity);
        return ApiResponse<PhotoMonitoringDto>.Ok(dto, "PhotoMonitoring updated successfully");
    }
}
