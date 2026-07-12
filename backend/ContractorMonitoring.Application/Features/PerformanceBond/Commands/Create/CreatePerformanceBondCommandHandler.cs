using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PerformanceBond;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PerformanceBond.Commands.Create;

public class CreatePerformanceBondCommandHandler : IRequestHandler<CreatePerformanceBondCommand, ApiResponse<PerformanceBondDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreatePerformanceBondCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PerformanceBondDto>> Handle(CreatePerformanceBondCommand command, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Domain.Entities.PerformanceBond>(command.Request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = command.UserId.ToString();
        entity.TenantId = command.TenantId;
        entity.IsDeleted = false;

        await _unitOfWork.PerformanceBonds.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<PerformanceBondDto>(entity);
        return ApiResponse<PerformanceBondDto>.Ok(dto, "PerformanceBond created successfully");
    }
}
