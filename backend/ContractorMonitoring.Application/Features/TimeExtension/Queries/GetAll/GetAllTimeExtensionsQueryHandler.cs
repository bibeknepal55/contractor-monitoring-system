using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.TimeExtension;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.TimeExtension.Queries.GetAll;

public class GetAllTimeExtensionsQueryHandler : IRequestHandler<GetAllTimeExtensionsQuery, PagedResponse<TimeExtensionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllTimeExtensionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<TimeExtensionDto>> Handle(GetAllTimeExtensionsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _unitOfWork.TimeExtensions.GetPagedAsync(request.Filter, predicate: x => x.TenantId == request.TenantId);
        var dtos = _mapper.Map<List<TimeExtensionDto>>(paged.Data);

        return new PagedResponse<TimeExtensionDto>
        {
            Data = dtos,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages,
            Message = "TimeExtension retrieved successfully"
        };
    }
}
