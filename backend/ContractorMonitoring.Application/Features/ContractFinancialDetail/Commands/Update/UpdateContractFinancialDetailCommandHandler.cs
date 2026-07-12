using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ContractFinancialDetail;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ContractFinancialDetail.Commands.Update;

public class UpdateContractFinancialDetailCommandHandler : IRequestHandler<UpdateContractFinancialDetailCommand, ApiResponse<ContractFinancialDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateContractFinancialDetailCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ContractFinancialDetailDto>> Handle(UpdateContractFinancialDetailCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.ContractFinancialDetails.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<ContractFinancialDetailDto>.Fail("Contract financial detail not found");

        _mapper.Map(command.Request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = command.UserId.ToString();
        entity.PendingPayment = entity.ContractAmount - (entity.TotalPaidAmount ?? 0);

        await _unitOfWork.ContractFinancialDetails.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var project = await _unitOfWork.Projects.GetByIdAsync(entity.ProjectId);
        var dto = _mapper.Map<ContractFinancialDetailDto>(entity);
        dto.ProjectName = project?.ProjectName ?? string.Empty;
        return ApiResponse<ContractFinancialDetailDto>.Ok(dto, "Contract financial detail updated successfully");
    }
}