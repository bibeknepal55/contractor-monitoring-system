using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ContractorOfficeDetail;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ContractorOfficeDetail.Queries.GetAll;

public class GetAllContractorOfficeDetailsQueryHandler : IRequestHandler<GetAllContractorOfficeDetailsQuery, PagedResponse<ContractorOfficeDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllContractorOfficeDetailsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<ContractorOfficeDetailDto>> Handle(GetAllContractorOfficeDetailsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _unitOfWork.ContractorOfficeDetails.GetPagedAsync(
            request.Filter,
            predicate: c => c.TenantId == request.TenantId
        );

        var dtos = _mapper.Map<List<ContractorOfficeDetailDto>>(paged.Data);

        return new PagedResponse<ContractorOfficeDetailDto>
        {
            Data = dtos,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages,
            Message = "Contractors retrieved successfully"
        };
    }
}