using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PriceAdjustment;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PriceAdjustment.Commands.Create;

public class CreatePriceAdjustmentCommandHandler : IRequestHandler<CreatePriceAdjustmentCommand, ApiResponse<PriceAdjustmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreatePriceAdjustmentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PriceAdjustmentDto>> Handle(CreatePriceAdjustmentCommand command, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Domain.Entities.PriceAdjustment>(command.Request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = command.UserId.ToString();
        entity.TenantId = command.TenantId;
        entity.IsDeleted = false;

        await _unitOfWork.PriceAdjustments.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<PriceAdjustmentDto>(entity);
        return ApiResponse<PriceAdjustmentDto>.Ok(dto, "PriceAdjustment created successfully");
    }
}
