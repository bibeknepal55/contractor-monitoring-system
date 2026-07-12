using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhotoMonitoring;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PhotoMonitoring.Commands.Create;

public class CreatePhotoMonitoringCommandHandler : IRequestHandler<CreatePhotoMonitoringCommand, ApiResponse<PhotoMonitoringDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreatePhotoMonitoringCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PhotoMonitoringDto>> Handle(CreatePhotoMonitoringCommand command, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Domain.Entities.PhotoMonitoring>(command.Request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = command.UserId.ToString();
        entity.TenantId = command.TenantId;
        entity.IsDeleted = false;

        await _unitOfWork.PhotoMonitorings.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<PhotoMonitoringDto>(entity);
        return ApiResponse<PhotoMonitoringDto>.Ok(dto, "PhotoMonitoring created successfully");
    }
}
