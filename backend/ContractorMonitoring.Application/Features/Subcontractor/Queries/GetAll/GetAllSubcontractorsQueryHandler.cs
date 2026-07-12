using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Subcontractor;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Subcontractor.Queries.GetAll;

public class GetAllSubcontractorsQueryHandler : IRequestHandler<GetAllSubcontractorsQuery, PagedResponse<SubcontractorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllSubcontractorsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<SubcontractorDto>> Handle(GetAllSubcontractorsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _unitOfWork.Subcontractors.GetPagedAsync(request.Filter, predicate: x => x.TenantId == request.TenantId);
        var dtos = _mapper.Map<List<SubcontractorDto>>(paged.Data);

        return new PagedResponse<SubcontractorDto>
        {
            Data = dtos,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages,
            Message = "Subcontractor retrieved successfully"
        };
    }
}
