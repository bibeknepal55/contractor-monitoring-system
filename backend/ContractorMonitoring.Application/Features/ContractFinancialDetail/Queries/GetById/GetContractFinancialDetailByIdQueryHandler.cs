using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ContractFinancialDetail;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ContractFinancialDetail.Queries.GetById;

public class GetContractFinancialDetailByIdQueryHandler : IRequestHandler<GetContractFinancialDetailByIdQuery, ApiResponse<ContractFinancialDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetContractFinancialDetailByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ContractFinancialDetailDto>> Handle(GetContractFinancialDetailByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.ContractFinancialDetails.GetByIdAsync(request.Id);
        if (entity == null)
            return ApiResponse<ContractFinancialDetailDto>.Fail("Contract financial detail not found");

        var project = await _unitOfWork.Projects.GetByIdAsync(entity.ProjectId);
        var dto = _mapper.Map<ContractFinancialDetailDto>(entity);
        dto.ProjectName = project?.ProjectName ?? string.Empty;
        return ApiResponse<ContractFinancialDetailDto>.Ok(dto, "Contract financial detail retrieved successfully");
    }
}