using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ApprovalWorkflow;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ApprovalWorkflow.Commands.Process;

public class ProcessApprovalCommandHandler : IRequestHandler<ProcessApprovalCommand, ApiResponse<ApprovalWorkflowDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProcessApprovalCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ApprovalWorkflowDto>> Handle(ProcessApprovalCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.ApprovalWorkflows.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<ApprovalWorkflowDto>.Fail("Approval request not found");

        if (entity.Status != "Pending")
            return ApiResponse<ApprovalWorkflowDto>.Fail("This request has already been processed");

        entity.PreviousStatus = entity.Status;
        entity.Action = command.Request.Action;
        entity.Status = command.Request.Action;
        entity.Comments = command.Request.Comments;
        entity.ApprovedBy = command.UserName;
        entity.ApprovalDate = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = command.UserId.ToString();

        await _unitOfWork.ApprovalWorkflows.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<ApprovalWorkflowDto>(entity);
        return ApiResponse<ApprovalWorkflowDto>.Ok(dto, $"Request {command.Request.Action.ToLower()} successfully");
    }
}