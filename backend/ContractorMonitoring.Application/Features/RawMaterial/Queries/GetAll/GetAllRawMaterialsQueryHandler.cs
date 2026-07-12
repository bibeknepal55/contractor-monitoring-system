using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RawMaterial;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.RawMaterial.Queries.GetAll;

public class GetAllRawMaterialsQueryHandler : IRequestHandler<GetAllRawMaterialsQuery, PagedResponse<RawMaterialDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllRawMaterialsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<RawMaterialDto>> Handle(GetAllRawMaterialsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _unitOfWork.RawMaterials.GetPagedAsync(request.Filter, predicate: x => x.TenantId == request.TenantId);
        var dtos = _mapper.Map<List<RawMaterialDto>>(paged.Data);

        return new PagedResponse<RawMaterialDto>
        {
            Data = dtos,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages,
            Message = "RawMaterial retrieved successfully"
        };
    }
}
