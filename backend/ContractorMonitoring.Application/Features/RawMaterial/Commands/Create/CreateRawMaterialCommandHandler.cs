using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RawMaterial;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.RawMaterial.Commands.Create;

public class CreateRawMaterialCommandHandler : IRequestHandler<CreateRawMaterialCommand, ApiResponse<RawMaterialDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateRawMaterialCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<RawMaterialDto>> Handle(CreateRawMaterialCommand command, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Domain.Entities.RawMaterial>(command.Request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = command.UserId.ToString();
        entity.TenantId = command.TenantId;
        entity.IsDeleted = false;

        await _unitOfWork.RawMaterials.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<RawMaterialDto>(entity);
        return ApiResponse<RawMaterialDto>.Ok(dto, "RawMaterial created successfully");
    }
}
