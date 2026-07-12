using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.DelayReason;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.DelayReason.Queries.GetById;

public class GetDelayReasonByIdQueryHandler : IRequestHandler<GetDelayReasonByIdQuery, ApiResponse<DelayReasonDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDelayReasonByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<DelayReasonDto>> Handle(GetDelayReasonByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.DelayReasons.GetByIdAsync(request.Id);
        if (entity == null)
            return ApiResponse<DelayReasonDto>.Fail("DelayReason not found");

        var dto = _mapper.Map<DelayReasonDto>(entity);
        return ApiResponse<DelayReasonDto>.Ok(dto, "DelayReason retrieved successfully");
    }
}
