using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PerformanceBond;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PerformanceBond.Queries.GetById;

public class GetPerformanceBondByIdQueryHandler : IRequestHandler<GetPerformanceBondByIdQuery, ApiResponse<PerformanceBondDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPerformanceBondByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PerformanceBondDto>> Handle(GetPerformanceBondByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.PerformanceBonds.GetByIdAsync(request.Id);
        if (entity == null)
            return ApiResponse<PerformanceBondDto>.Fail("PerformanceBond not found");

        var dto = _mapper.Map<PerformanceBondDto>(entity);
        return ApiResponse<PerformanceBondDto>.Ok(dto, "PerformanceBond retrieved successfully");
    }
}
