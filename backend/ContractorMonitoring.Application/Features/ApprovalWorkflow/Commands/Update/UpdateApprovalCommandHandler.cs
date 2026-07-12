using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ApprovalWorkflow;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ApprovalWorkflow.Commands.Update;

public class UpdateApprovalCommandHandler : IRequestHandler<UpdateApprovalCommand, ApiResponse<ApprovalWorkflowDto>>
{
    private readonly IApprovalRepository _repository;

    public UpdateApprovalCommandHandler(IApprovalRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<ApprovalWorkflowDto>> Handle(UpdateApprovalCommand request, CancellationToken cancellationToken)
    {
        var approval = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (approval == null)
            return new ApiResponse<ApprovalWorkflowDto>
            {
                Success = false,
                Message = "Approval request not found"
            };

        if (approval.Status != "Pending")
            return new ApiResponse<ApprovalWorkflowDto>
            {
                Success = false,
                Message = "Only pending requests can be edited"
            };

        if (approval.RequestedBy != request.UserName)
            return new ApiResponse<ApprovalWorkflowDto>
            {
                Success = false,
                Message = "You can only edit your own requests"
            };

        approval.ModuleName = request.Request.ModuleName;

        // Convert string RecordId to Guid
        if (Guid.TryParse(request.Request.RecordId, out var recordId))
        {
            approval.RecordId = recordId;
        }

        approval.RecordTitle = request.Request.RecordTitle;
        approval.Comments = request.Request.Comments;
        approval.ApprovalLevel = request.Request.ApprovalLevel;

        await _repository.UpdateAsync(approval, cancellationToken);

        var dto = new ApprovalWorkflowDto
        {
            Id = approval.Id,
            ModuleName = approval.ModuleName,
            RecordId = approval.RecordId,
            RecordTitle = approval.RecordTitle,
            Comments = approval.Comments,
            ApprovalLevel = approval.ApprovalLevel,
            Status = approval.Status,
            RequestedBy = approval.RequestedBy,
            CreatedAt = approval.CreatedAt
        };

        return new ApiResponse<ApprovalWorkflowDto>
        {
            Success = true,
            Data = dto,
            Message = "Request updated successfully"
        };
    }
}