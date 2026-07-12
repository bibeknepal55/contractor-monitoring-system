using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RawMaterial;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.RawMaterial.Queries.GetById;

public class GetRawMaterialByIdQueryHandler : IRequestHandler<GetRawMaterialByIdQuery, ApiResponse<RawMaterialDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetRawMaterialByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<RawMaterialDto>> Handle(GetRawMaterialByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.RawMaterials.GetByIdAsync(request.Id);
        if (entity == null)
            return ApiResponse<RawMaterialDto>.Fail("RawMaterial not found");

        var dto = _mapper.Map<RawMaterialDto>(entity);
        return ApiResponse<RawMaterialDto>.Ok(dto, "RawMaterial retrieved successfully");
    }
}
