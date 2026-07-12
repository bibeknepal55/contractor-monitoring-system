using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.DelayReason;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.DelayReason.Commands.Create;

public class CreateDelayReasonCommandHandler : IRequestHandler<CreateDelayReasonCommand, ApiResponse<DelayReasonDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateDelayReasonCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<DelayReasonDto>> Handle(CreateDelayReasonCommand command, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Domain.Entities.DelayReason>(command.Request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = command.UserId.ToString();
        entity.TenantId = command.TenantId;
        entity.IsDeleted = false;

        await _unitOfWork.DelayReasons.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<DelayReasonDto>(entity);
        return ApiResponse<DelayReasonDto>.Ok(dto, "DelayReason created successfully");
    }
}
