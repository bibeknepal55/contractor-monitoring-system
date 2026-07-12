using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.AdvancePaymentGuarantee;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.AdvancePaymentGuarantee.Queries.GetById;

public class GetAdvancePaymentGuaranteeByIdQueryHandler : IRequestHandler<GetAdvancePaymentGuaranteeByIdQuery, ApiResponse<AdvancePaymentGuaranteeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAdvancePaymentGuaranteeByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<AdvancePaymentGuaranteeDto>> Handle(GetAdvancePaymentGuaranteeByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.AdvancePaymentGuarantees.GetByIdAsync(request.Id);
        if (entity == null)
            return ApiResponse<AdvancePaymentGuaranteeDto>.Fail("AdvancePaymentGuarantee not found");

        var dto = _mapper.Map<AdvancePaymentGuaranteeDto>(entity);
        return ApiResponse<AdvancePaymentGuaranteeDto>.Ok(dto, "AdvancePaymentGuarantee retrieved successfully");
    }
}
