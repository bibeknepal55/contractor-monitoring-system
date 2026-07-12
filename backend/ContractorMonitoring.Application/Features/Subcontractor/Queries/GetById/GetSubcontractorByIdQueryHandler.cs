using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Subcontractor;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Subcontractor.Queries.GetById;

public class GetSubcontractorByIdQueryHandler : IRequestHandler<GetSubcontractorByIdQuery, ApiResponse<SubcontractorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSubcontractorByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<SubcontractorDto>> Handle(GetSubcontractorByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Subcontractors.GetByIdAsync(request.Id);
        if (entity == null)
            return ApiResponse<SubcontractorDto>.Fail("Subcontractor not found");

        var dto = _mapper.Map<SubcontractorDto>(entity);
        return ApiResponse<SubcontractorDto>.Ok(dto, "Subcontractor retrieved successfully");
    }
}
