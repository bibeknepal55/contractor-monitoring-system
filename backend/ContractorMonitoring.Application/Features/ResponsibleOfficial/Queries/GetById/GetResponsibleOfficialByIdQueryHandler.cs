using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ResponsibleOfficial;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ResponsibleOfficial.Queries.GetById;

public class GetResponsibleOfficialByIdQueryHandler : IRequestHandler<GetResponsibleOfficialByIdQuery, ApiResponse<ResponsibleOfficialDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetResponsibleOfficialByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ResponsibleOfficialDto>> Handle(GetResponsibleOfficialByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.ResponsibleOfficials.GetByIdAsync(request.Id);
        if (entity == null)
            return ApiResponse<ResponsibleOfficialDto>.Fail("ResponsibleOfficial not found");

        var dto = _mapper.Map<ResponsibleOfficialDto>(entity);
        return ApiResponse<ResponsibleOfficialDto>.Ok(dto, "ResponsibleOfficial retrieved successfully");
    }
}
