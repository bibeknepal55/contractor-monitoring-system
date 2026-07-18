using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhysicalProgress;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PhysicalProgress.Queries.GetAll;

public class GetAllPhysicalProgressesQueryHandler : IRequestHandler<GetAllPhysicalProgressesQuery, PagedResponse<PhysicalProgressDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllPhysicalProgressesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<PhysicalProgressDto>> Handle(GetAllPhysicalProgressesQuery request, CancellationToken cancellationToken)
    {
        var paged = await _unitOfWork.PhysicalProgresses.GetPagedAsync(request.Filter, predicate: x => x.TenantId == request.TenantId);
        var dtos = _mapper.Map<List<PhysicalProgressDto>>(paged.Data);
        foreach (var dto in dtos)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId);
            dto.ProjectName = project?.ProjectName ?? "Unknown";
        }

        return new PagedResponse<PhysicalProgressDto>
        {
            Data = dtos,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages,
            Message = "PhysicalProgress retrieved successfully"
        };
    }
}

