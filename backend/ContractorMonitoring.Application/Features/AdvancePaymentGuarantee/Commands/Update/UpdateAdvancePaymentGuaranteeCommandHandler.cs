using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.AdvancePaymentGuarantee;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.AdvancePaymentGuarantee.Commands.Update;

public class UpdateAdvancePaymentGuaranteeCommandHandler : IRequestHandler<UpdateAdvancePaymentGuaranteeCommand, ApiResponse<AdvancePaymentGuaranteeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateAdvancePaymentGuaranteeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<AdvancePaymentGuaranteeDto>> Handle(UpdateAdvancePaymentGuaranteeCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.AdvancePaymentGuarantees.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<AdvancePaymentGuaranteeDto>.Fail("AdvancePaymentGuarantee not found");

        _mapper.Map(command.Request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = command.UserId.ToString();

        await _unitOfWork.AdvancePaymentGuarantees.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<AdvancePaymentGuaranteeDto>(entity);
        return ApiResponse<AdvancePaymentGuaranteeDto>.Ok(dto, "AdvancePaymentGuarantee updated successfully");
    }
}
