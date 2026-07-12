using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Reports;

namespace ContractorMonitoring.Application.Features.Reports.Queries.GetReports;

public record GetReportsQuery : IRequest<ApiResponse<object>>
{
    public ReportRequestDto Request { get; init; } = new();
    public Guid TenantId { get; init; }
}