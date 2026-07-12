using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PriceAdjustment;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PriceAdjustment.Queries.GetAll;

public class GetAllPriceAdjustmentsQueryHandler : IRequestHandler<GetAllPriceAdjustmentsQuery, PagedResponse<PriceAdjustmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllPriceAdjustmentsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<PriceAdjustmentDto>> Handle(GetAllPriceAdjustmentsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _unitOfWork.PriceAdjustments.GetPagedAsync(request.Filter, predicate: x => x.TenantId == request.TenantId);
        var dtos = _mapper.Map<List<PriceAdjustmentDto>>(paged.Data);

        return new PagedResponse<PriceAdjustmentDto>
        {
            Data = dtos,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages,
            Message = "PriceAdjustment retrieved successfully"
        };
    }
}
