using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Dashboard;

namespace ContractorMonitoring.Application.Features.Dashboard.Queries.GetExecutiveDashboard;

public record GetExecutiveDashboardQuery : IRequest<ApiResponse<ExecutiveDashboardDto>>
{
    public Guid TenantId { get; init; }
}