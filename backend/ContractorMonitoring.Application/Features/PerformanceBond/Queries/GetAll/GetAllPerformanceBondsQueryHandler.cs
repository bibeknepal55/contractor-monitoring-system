using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PerformanceBond;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PerformanceBond.Queries.GetAll;

public class GetAllPerformanceBondsQueryHandler : IRequestHandler<GetAllPerformanceBondsQuery, PagedResponse<PerformanceBondDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllPerformanceBondsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<PerformanceBondDto>> Handle(GetAllPerformanceBondsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _unitOfWork.PerformanceBonds.GetPagedAsync(request.Filter, predicate: x => x.TenantId == request.TenantId);
        var dtos = _mapper.Map<List<PerformanceBondDto>>(paged.Data);
        foreach (var dto in dtos)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId);
            dto.ProjectName = project?.ProjectName ?? "Unknown";

        }

        return new PagedResponse<PerformanceBondDto>
        {
            Data = dtos,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages,
            Message = "PerformanceBond retrieved successfully"
        };
    }
}
