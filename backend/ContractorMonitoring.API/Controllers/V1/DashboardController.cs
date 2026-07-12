using Asp.Versioning;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Dashboard;
using ContractorMonitoring.Application.Features.Dashboard.Queries.GetExecutiveDashboard;
using ContractorMonitoring.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/dashboard")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    public DashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet("executive")]
    [Authorize(Policy = Permissions.Dashboard.View)]
    public async Task<ActionResult<ApiResponse<ExecutiveDashboardDto>>> GetExecutiveDashboard()
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var result = await _mediator.Send(new GetExecutiveDashboardQuery { TenantId = tenantId });
        return Ok(result);
    }
}