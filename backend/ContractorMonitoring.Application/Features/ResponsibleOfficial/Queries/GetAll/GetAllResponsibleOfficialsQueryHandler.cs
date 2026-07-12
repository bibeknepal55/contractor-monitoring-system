using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ResponsibleOfficial;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ResponsibleOfficial.Queries.GetAll;

public class GetAllResponsibleOfficialsQueryHandler : IRequestHandler<GetAllResponsibleOfficialsQuery, PagedResponse<ResponsibleOfficialDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllResponsibleOfficialsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<ResponsibleOfficialDto>> Handle(GetAllResponsibleOfficialsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _unitOfWork.ResponsibleOfficials.GetPagedAsync(request.Filter, predicate: x => x.TenantId == request.TenantId);
        var dtos = _mapper.Map<List<ResponsibleOfficialDto>>(paged.Data);

        return new PagedResponse<ResponsibleOfficialDto>
        {
            Data = dtos,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages,
            Message = "ResponsibleOfficial retrieved successfully"
        };
    }
}
