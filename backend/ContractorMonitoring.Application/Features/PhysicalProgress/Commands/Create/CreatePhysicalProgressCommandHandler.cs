using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhysicalProgress;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PhysicalProgress.Commands.Create;

public class CreatePhysicalProgressCommandHandler : IRequestHandler<CreatePhysicalProgressCommand, ApiResponse<PhysicalProgressDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreatePhysicalProgressCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PhysicalProgressDto>> Handle(CreatePhysicalProgressCommand command, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Domain.Entities.PhysicalProgress>(command.Request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = command.UserId.ToString();
        entity.TenantId = command.TenantId;
        entity.IsDeleted = false;

        await _unitOfWork.PhysicalProgresses.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<PhysicalProgressDto>(entity);
        return ApiResponse<PhysicalProgressDto>.Ok(dto, "PhysicalProgress created successfully");
    }
}

