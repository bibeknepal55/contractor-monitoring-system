using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ApprovalWorkflow;

namespace ContractorMonitoring.Application.Features.ApprovalWorkflow.Commands.Create;

public record CreateApprovalCommand : IRequest<ApiResponse<ApprovalWorkflowDto>>
{
    public CreateApprovalRequestDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
    public string UserName { get; init; } = string.Empty;
}