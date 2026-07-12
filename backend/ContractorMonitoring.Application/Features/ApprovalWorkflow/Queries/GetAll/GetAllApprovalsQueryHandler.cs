using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ApprovalWorkflow;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ApprovalWorkflow.Queries.GetAll;

public class GetAllApprovalsQueryHandler : IRequestHandler<GetAllApprovalsQuery, PagedResponse<ApprovalWorkflowDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllApprovalsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<ApprovalWorkflowDto>> Handle(GetAllApprovalsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _unitOfWork.ApprovalWorkflows.GetPagedAsync(
            request.Filter,
            predicate: a => a.TenantId == request.TenantId
        );

        var dtos = _mapper.Map<List<ApprovalWorkflowDto>>(paged.Data);

        return new PagedResponse<ApprovalWorkflowDto>
        {
            Data = dtos,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages,
            Message = "Approvals retrieved successfully"
        };
    }
}