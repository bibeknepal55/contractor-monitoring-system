using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RawMaterial;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.RawMaterial.Commands.Update;

public class UpdateRawMaterialCommandHandler : IRequestHandler<UpdateRawMaterialCommand, ApiResponse<RawMaterialDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateRawMaterialCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<RawMaterialDto>> Handle(UpdateRawMaterialCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.RawMaterials.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<RawMaterialDto>.Fail("RawMaterial not found");

        _mapper.Map(command.Request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = command.UserId.ToString();

        await _unitOfWork.RawMaterials.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<RawMaterialDto>(entity);
        return ApiResponse<RawMaterialDto>.Ok(dto, "RawMaterial updated successfully");
    }
}
