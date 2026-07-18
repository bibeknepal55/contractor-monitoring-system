using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhotoMonitoring;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PhotoMonitoring.Queries.GetAll;

public class GetAllPhotoMonitoringsQueryHandler : IRequestHandler<GetAllPhotoMonitoringsQuery, PagedResponse<PhotoMonitoringDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllPhotoMonitoringsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<PhotoMonitoringDto>> Handle(GetAllPhotoMonitoringsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _unitOfWork.PhotoMonitorings.GetPagedAsync(request.Filter, predicate: x => x.TenantId == request.TenantId);
        var dtos = _mapper.Map<List<PhotoMonitoringDto>>(paged.Data);
        foreach (var dto in dtos)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId);
            dto.ProjectName = project?.ProjectName ?? "Unknown";
        }

        return new PagedResponse<PhotoMonitoringDto>
        {
            Data = dtos,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages,
            Message = "PhotoMonitoring retrieved successfully"
        };
    }
}
