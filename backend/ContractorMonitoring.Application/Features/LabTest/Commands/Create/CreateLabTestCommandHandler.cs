using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.LabTest;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.LabTest.Commands.Create;

public class CreateLabTestCommandHandler : IRequestHandler<CreateLabTestCommand, ApiResponse<LabTestDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateLabTestCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<LabTestDto>> Handle(CreateLabTestCommand command, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Domain.Entities.LabTest>(command.Request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = command.UserId.ToString();
        entity.TenantId = command.TenantId;
        entity.IsDeleted = false;

        await _unitOfWork.LabTests.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<LabTestDto>(entity);
        return ApiResponse<LabTestDto>.Ok(dto, "LabTest created successfully");
    }
}
