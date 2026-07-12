using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Subcontractor;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Subcontractor.Commands.Create;

public class CreateSubcontractorCommandHandler : IRequestHandler<CreateSubcontractorCommand, ApiResponse<SubcontractorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSubcontractorCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<SubcontractorDto>> Handle(CreateSubcontractorCommand command, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Domain.Entities.Subcontractor>(command.Request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = command.UserId.ToString();
        entity.TenantId = command.TenantId;
        entity.IsDeleted = false;

        await _unitOfWork.Subcontractors.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<SubcontractorDto>(entity);
        return ApiResponse<SubcontractorDto>.Ok(dto, "Subcontractor created successfully");
    }
}
