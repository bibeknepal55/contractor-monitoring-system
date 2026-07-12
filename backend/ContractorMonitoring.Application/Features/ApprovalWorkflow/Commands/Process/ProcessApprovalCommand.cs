using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ApprovalWorkflow;

namespace ContractorMonitoring.Application.Features.ApprovalWorkflow.Commands.Process;

public record ProcessApprovalCommand : IRequest<ApiResponse<ApprovalWorkflowDto>>
{
    public Guid Id { get; init; }
    public ProcessApprovalDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
}