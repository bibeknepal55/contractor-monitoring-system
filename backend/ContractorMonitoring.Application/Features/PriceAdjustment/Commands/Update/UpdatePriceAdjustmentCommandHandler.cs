using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PriceAdjustment;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PriceAdjustment.Commands.Update;

public class UpdatePriceAdjustmentCommandHandler : IRequestHandler<UpdatePriceAdjustmentCommand, ApiResponse<PriceAdjustmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdatePriceAdjustmentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PriceAdjustmentDto>> Handle(UpdatePriceAdjustmentCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.PriceAdjustments.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<PriceAdjustmentDto>.Fail("PriceAdjustment not found");

        _mapper.Map(command.Request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = command.UserId.ToString();

        await _unitOfWork.PriceAdjustments.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<PriceAdjustmentDto>(entity);
        return ApiResponse<PriceAdjustmentDto>.Ok(dto, "PriceAdjustment updated successfully");
    }
}
