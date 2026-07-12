using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ContractorOfficeDetail;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ContractorOfficeDetail.Queries.GetById;

public class GetContractorOfficeDetailByIdQueryHandler : IRequestHandler<GetContractorOfficeDetailByIdQuery, ApiResponse<ContractorOfficeDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetContractorOfficeDetailByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ContractorOfficeDetailDto>> Handle(GetContractorOfficeDetailByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.ContractorOfficeDetails.GetByIdAsync(request.Id);
        if (entity == null)
            return ApiResponse<ContractorOfficeDetailDto>.Fail("Contractor not found");

        var dto = _mapper.Map<ContractorOfficeDetailDto>(entity);
        return ApiResponse<ContractorOfficeDetailDto>.Ok(dto, "Contractor retrieved successfully");
    }
}