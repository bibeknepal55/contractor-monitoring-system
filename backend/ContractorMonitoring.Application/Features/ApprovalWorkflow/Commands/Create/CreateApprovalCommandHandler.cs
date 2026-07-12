using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ApprovalWorkflow;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ApprovalWorkflow.Commands.Create;

public class CreateApprovalCommandHandler : IRequestHandler<CreateApprovalCommand, ApiResponse<ApprovalWorkflowDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateApprovalCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ApprovalWorkflowDto>> Handle(CreateApprovalCommand command, CancellationToken cancellationToken)
    {
        var entity = new Domain.Entities.ApprovalWorkflow
        {
            Id = Guid.NewGuid(),
            ModuleName = command.Request.ModuleName,
            RecordId = command.Request.RecordId,
            RecordTitle = command.Request.RecordTitle,
            Action = "Submitted",
            Comments = command.Request.Comments,
            RequestedBy = command.UserName,
            ApprovalLevel = command.Request.ApprovalLevel,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = command.UserId.ToString(),
            TenantId = command.TenantId
        };

        await _unitOfWork.ApprovalWorkflows.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<ApprovalWorkflowDto>(entity);
        return ApiResponse<ApprovalWorkflowDto>.Ok(dto, "Approval request submitted successfully");
    }
}