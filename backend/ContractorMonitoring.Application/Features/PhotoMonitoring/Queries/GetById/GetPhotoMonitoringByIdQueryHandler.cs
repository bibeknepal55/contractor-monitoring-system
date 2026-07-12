using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhotoMonitoring;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PhotoMonitoring.Queries.GetById;

public class GetPhotoMonitoringByIdQueryHandler : IRequestHandler<GetPhotoMonitoringByIdQuery, ApiResponse<PhotoMonitoringDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPhotoMonitoringByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PhotoMonitoringDto>> Handle(GetPhotoMonitoringByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.PhotoMonitorings.GetByIdAsync(request.Id);
        if (entity == null)
            return ApiResponse<PhotoMonitoringDto>.Fail("PhotoMonitoring not found");

        var dto = _mapper.Map<PhotoMonitoringDto>(entity);
        return ApiResponse<PhotoMonitoringDto>.Ok(dto, "PhotoMonitoring retrieved successfully");
    }
}
