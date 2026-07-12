using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.LabTest;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.LabTest.Queries.GetAll;

public class GetAllLabTestsQueryHandler : IRequestHandler<GetAllLabTestsQuery, PagedResponse<LabTestDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllLabTestsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<LabTestDto>> Handle(GetAllLabTestsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _unitOfWork.LabTests.GetPagedAsync(request.Filter, predicate: x => x.TenantId == request.TenantId);
        var dtos = _mapper.Map<List<LabTestDto>>(paged.Data);

        return new PagedResponse<LabTestDto>
        {
            Data = dtos,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages,
            Message = "LabTest retrieved successfully"
        };
    }
}
