using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ContractFinancialDetail;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ContractFinancialDetail.Commands.Create;

public class CreateContractFinancialDetailCommandHandler : IRequestHandler<CreateContractFinancialDetailCommand, ApiResponse<ContractFinancialDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateContractFinancialDetailCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ContractFinancialDetailDto>> Handle(CreateContractFinancialDetailCommand command, CancellationToken cancellationToken)
    {
        var projectExists = await _unitOfWork.Projects.ExistsAsync(p => p.Id == command.Request.ProjectId);
        if (!projectExists)
            return ApiResponse<ContractFinancialDetailDto>.Fail("Project not found");

        var entity = _mapper.Map<Domain.Entities.ContractFinancialDetail>(command.Request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = command.UserId.ToString();
        entity.TenantId = command.TenantId;
        entity.PaymentStatus = "Pending";

        await _unitOfWork.ContractFinancialDetails.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var project = await _unitOfWork.Projects.GetByIdAsync(entity.ProjectId);
        var dto = _mapper.Map<ContractFinancialDetailDto>(entity);
        dto.ProjectName = project?.ProjectName ?? string.Empty;
        return ApiResponse<ContractFinancialDetailDto>.Ok(dto, "Contract financial detail created successfully");
    }
}