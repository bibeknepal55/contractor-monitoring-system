using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PriceAdjustment;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PriceAdjustment.Queries.GetById;

public class GetPriceAdjustmentByIdQueryHandler : IRequestHandler<GetPriceAdjustmentByIdQuery, ApiResponse<PriceAdjustmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPriceAdjustmentByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PriceAdjustmentDto>> Handle(GetPriceAdjustmentByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.PriceAdjustments.GetByIdAsync(request.Id);
        if (entity == null)
            return ApiResponse<PriceAdjustmentDto>.Fail("PriceAdjustment not found");

        var dto = _mapper.Map<PriceAdjustmentDto>(entity);
        return ApiResponse<PriceAdjustmentDto>.Ok(dto, "PriceAdjustment retrieved successfully");
    }
}
