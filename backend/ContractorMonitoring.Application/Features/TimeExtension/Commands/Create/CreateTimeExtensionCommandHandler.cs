using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.TimeExtension;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.TimeExtension.Commands.Create;

public class CreateTimeExtensionCommandHandler : IRequestHandler<CreateTimeExtensionCommand, ApiResponse<TimeExtensionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateTimeExtensionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<TimeExtensionDto>> Handle(CreateTimeExtensionCommand command, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Domain.Entities.TimeExtension>(command.Request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = command.UserId.ToString();
        entity.TenantId = command.TenantId;
        entity.IsDeleted = false;

        await _unitOfWork.TimeExtensions.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<TimeExtensionDto>(entity);
        return ApiResponse<TimeExtensionDto>.Ok(dto, "TimeExtension created successfully");
    }
}
