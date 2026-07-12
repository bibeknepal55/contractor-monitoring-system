using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ApprovalWorkflow;

namespace ContractorMonitoring.Application.Features.ApprovalWorkflow.Queries.GetAll;

public record GetAllApprovalsQuery : IRequest<PagedResponse<ApprovalWorkflowDto>>
{
    public PaginationFilter Filter { get; init; } = new();
    public Guid TenantId { get; init; }
}