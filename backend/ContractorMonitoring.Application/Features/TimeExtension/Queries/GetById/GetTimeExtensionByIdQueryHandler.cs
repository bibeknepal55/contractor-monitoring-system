using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.TimeExtension;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.TimeExtension.Queries.GetById;

public class GetTimeExtensionByIdQueryHandler : IRequestHandler<GetTimeExtensionByIdQuery, ApiResponse<TimeExtensionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTimeExtensionByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<TimeExtensionDto>> Handle(GetTimeExtensionByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.TimeExtensions.GetByIdAsync(request.Id);
        if (entity == null)
            return ApiResponse<TimeExtensionDto>.Fail("TimeExtension not found");

        var dto = _mapper.Map<TimeExtensionDto>(entity);
        return ApiResponse<TimeExtensionDto>.Ok(dto, "TimeExtension retrieved successfully");
    }
}
