using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.DelayReason;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.DelayReason.Queries.GetAll;

public class GetAllDelayReasonsQueryHandler : IRequestHandler<GetAllDelayReasonsQuery, PagedResponse<DelayReasonDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllDelayReasonsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<DelayReasonDto>> Handle(GetAllDelayReasonsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _unitOfWork.DelayReasons.GetPagedAsync(request.Filter, predicate: x => x.TenantId == request.TenantId);
        var dtos = _mapper.Map<List<DelayReasonDto>>(paged.Data);
        foreach (var dto in dtos)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId);
            dto.ProjectName = project?.ProjectName ?? "Unknown";
        }

        return new PagedResponse<DelayReasonDto>
        {
            Data = dtos,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages,
            Message = "DelayReason retrieved successfully"
        };
    }
}
