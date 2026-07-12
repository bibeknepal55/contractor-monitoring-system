using Asp.Versioning;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Reports;
using ContractorMonitoring.Application.Features.Reports.Queries.GetReports;
using ContractorMonitoring.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reports")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ReportsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("generate")]
    [Authorize(Policy = Permissions.Reports.View)]
    public async Task<ActionResult<ApiResponse<object>>> GenerateReport([FromBody] ReportRequestDto request)
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var result = await _mediator.Send(new GetReportsQuery { Request = request, TenantId = tenantId });
        return Ok(result);
    }

    [HttpGet("types")]
    [Authorize(Policy = Permissions.Reports.View)]
    public ActionResult<ApiResponse<object>> GetReportTypes()
    {
        var reportTypes = new[]
        {
            new { Id = "contractor-wise", Name = "Contractor-wise Report" },
            new { Id = "project-wise", Name = "Project-wise Report" },
            new { Id = "delay-analysis", Name = "Delay Analysis Report" },
            new { Id = "pb-apg", Name = "PB/APG Report" },
            new { Id = "time-extension", Name = "Time Extension Report" },
            new { Id = "payment-pending", Name = "Payment Pending Report" }
        };

        return Ok(ApiResponse<object>.Ok(reportTypes, "Report types retrieved"));
    }
}