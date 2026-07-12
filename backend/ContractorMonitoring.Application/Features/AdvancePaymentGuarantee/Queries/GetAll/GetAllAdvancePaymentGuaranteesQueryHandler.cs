using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.AdvancePaymentGuarantee;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.AdvancePaymentGuarantee.Queries.GetAll;

public class GetAllAdvancePaymentGuaranteesQueryHandler : IRequestHandler<GetAllAdvancePaymentGuaranteesQuery, PagedResponse<AdvancePaymentGuaranteeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllAdvancePaymentGuaranteesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<AdvancePaymentGuaranteeDto>> Handle(GetAllAdvancePaymentGuaranteesQuery request, CancellationToken cancellationToken)
    {
        var paged = await _unitOfWork.AdvancePaymentGuarantees.GetPagedAsync(request.Filter, predicate: x => x.TenantId == request.TenantId);
        var dtos = _mapper.Map<List<AdvancePaymentGuaranteeDto>>(paged.Data);

        return new PagedResponse<AdvancePaymentGuaranteeDto>
        {
            Data = dtos,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages,
            Message = "AdvancePaymentGuarantee retrieved successfully"
        };
    }
}
