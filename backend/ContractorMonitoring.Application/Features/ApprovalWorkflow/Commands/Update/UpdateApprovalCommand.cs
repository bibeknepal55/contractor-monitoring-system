using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ApprovalWorkflow;

namespace ContractorMonitoring.Application.Features.ApprovalWorkflow.Commands.Update;

public class UpdateApprovalCommand : IRequest<ApiResponse<ApprovalWorkflowDto>>
{
    public Guid Id { get; set; }
    public UpdateApprovalRequestDto Request { get; set; } = new();
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
}