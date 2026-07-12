using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ResponsibleOfficial;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ResponsibleOfficial.Commands.Create;

public class CreateResponsibleOfficialCommandHandler : IRequestHandler<CreateResponsibleOfficialCommand, ApiResponse<ResponsibleOfficialDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateResponsibleOfficialCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ResponsibleOfficialDto>> Handle(CreateResponsibleOfficialCommand command, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Domain.Entities.ResponsibleOfficial>(command.Request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = command.UserId.ToString();
        entity.TenantId = command.TenantId;
        entity.IsDeleted = false;

        await _unitOfWork.ResponsibleOfficials.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<ResponsibleOfficialDto>(entity);
        return ApiResponse<ResponsibleOfficialDto>.Ok(dto, "ResponsibleOfficial created successfully");
    }
}
