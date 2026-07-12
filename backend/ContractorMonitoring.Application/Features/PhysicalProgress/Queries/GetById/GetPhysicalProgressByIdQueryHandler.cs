using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhysicalProgress;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PhysicalProgress.Queries.GetById;

public class GetPhysicalProgressByIdQueryHandler : IRequestHandler<GetPhysicalProgressByIdQuery, ApiResponse<PhysicalProgressDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPhysicalProgressByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PhysicalProgressDto>> Handle(GetPhysicalProgressByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.PhysicalProgresses.GetByIdAsync(request.Id);
        if (entity == null)
            return ApiResponse<PhysicalProgressDto>.Fail("PhysicalProgress not found");

        var dto = _mapper.Map<PhysicalProgressDto>(entity);
        return ApiResponse<PhysicalProgressDto>.Ok(dto, "PhysicalProgress retrieved successfully");
    }
}

