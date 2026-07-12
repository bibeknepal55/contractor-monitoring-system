using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ContractFinancialDetail;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ContractFinancialDetail.Queries.GetAll;

public class GetAllContractFinancialDetailsQueryHandler : IRequestHandler<GetAllContractFinancialDetailsQuery, PagedResponse<ContractFinancialDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllContractFinancialDetailsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<ContractFinancialDetailDto>> Handle(GetAllContractFinancialDetailsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _unitOfWork.ContractFinancialDetails.GetPagedAsync(request.Filter, predicate: c => c.TenantId == request.TenantId);
        var dtos = _mapper.Map<List<ContractFinancialDetailDto>>(paged.Data);

        foreach (var dto in dtos)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId);
            dto.ProjectName = project?.ProjectName ?? string.Empty;
        }

        return new PagedResponse<ContractFinancialDetailDto>
        {
            Data = dtos,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages,
            Message = "Contract financial details retrieved successfully"
        };
    }
}