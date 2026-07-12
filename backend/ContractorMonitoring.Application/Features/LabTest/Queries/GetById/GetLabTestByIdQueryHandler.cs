using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.LabTest;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.LabTest.Queries.GetById;

public class GetLabTestByIdQueryHandler : IRequestHandler<GetLabTestByIdQuery, ApiResponse<LabTestDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetLabTestByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<LabTestDto>> Handle(GetLabTestByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.LabTests.GetByIdAsync(request.Id);
        if (entity == null)
            return ApiResponse<LabTestDto>.Fail("LabTest not found");

        var dto = _mapper.Map<LabTestDto>(entity);
        return ApiResponse<LabTestDto>.Ok(dto, "LabTest retrieved successfully");
    }
}
