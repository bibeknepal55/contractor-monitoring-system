using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.AdvancePaymentGuarantee;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.AdvancePaymentGuarantee.Commands.Create;

public class CreateAdvancePaymentGuaranteeCommandHandler : IRequestHandler<CreateAdvancePaymentGuaranteeCommand, ApiResponse<AdvancePaymentGuaranteeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateAdvancePaymentGuaranteeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<AdvancePaymentGuaranteeDto>> Handle(CreateAdvancePaymentGuaranteeCommand command, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Domain.Entities.AdvancePaymentGuarantee>(command.Request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = command.UserId.ToString();
        entity.TenantId = command.TenantId;
        entity.IsDeleted = false;

        await _unitOfWork.AdvancePaymentGuarantees.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<AdvancePaymentGuaranteeDto>(entity);
        return ApiResponse<AdvancePaymentGuaranteeDto>.Ok(dto, "AdvancePaymentGuarantee created successfully");
    }
}
