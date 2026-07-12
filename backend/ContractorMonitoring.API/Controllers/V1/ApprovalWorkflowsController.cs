using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ApprovalWorkflow;
using ContractorMonitoring.Application.Features.ApprovalWorkflow.Commands.Create;
using ContractorMonitoring.Application.Features.ApprovalWorkflow.Commands.Process;
using ContractorMonitoring.Application.Features.ApprovalWorkflow.Commands.Update;
using ContractorMonitoring.Application.Features.ApprovalWorkflow.Queries.GetAll;
using ContractorMonitoring.Domain.Constants;
using Asp.Versioning;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/approvals")]
[ApiController]
public class ApprovalWorkflowsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ApprovalWorkflowsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = Permissions.ApprovalWorkflow.View)]
    public async Task<ActionResult<PagedResponse<ApprovalWorkflowDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = "asc")
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        return Ok(await _mediator.Send(new GetAllApprovalsQuery
        {
            Filter = new PaginationFilter { Page = page, PageSize = pageSize, Search = search, SortBy = sortBy, SortOrder = sortOrder },
            TenantId = tenantId
        }));
    }

    [HttpPost("submit")]
    [Authorize(Policy = Permissions.ApprovalWorkflow.Create)]
    public async Task<ActionResult<ApiResponse<ApprovalWorkflowDto>>> Submit([FromBody] CreateApprovalRequestDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var userName = User.FindFirst(ClaimTypes.GivenName)?.Value + " " + User.FindFirst(ClaimTypes.Surname)?.Value;

        var result = await _mediator.Send(new CreateApprovalCommand
        {
            Request = request,
            UserId = userId,
            TenantId = tenantId,
            UserName = userName
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ========== UPDATE ENDPOINT ==========
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.ApprovalWorkflow.Create)]
    public async Task<ActionResult<ApiResponse<ApprovalWorkflowDto>>> Update(Guid id, [FromBody] UpdateApprovalRequestDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var userName = User.FindFirst(ClaimTypes.GivenName)?.Value + " " + User.FindFirst(ClaimTypes.Surname)?.Value;

        var result = await _mediator.Send(new UpdateApprovalCommand
        {
            Id = id,
            Request = request,
            UserId = userId,
            UserName = userName
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ========== PROCESS ENDPOINT (Approve/Reject) ==========
    [HttpPut("{id:guid}/process")]
    [Authorize(Policy = Permissions.ApprovalWorkflow.Approve, Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<ApprovalWorkflowDto>>> Process(Guid id, [FromBody] ProcessApprovalDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var userName = User.FindFirst(ClaimTypes.GivenName)?.Value + " " + User.FindFirst(ClaimTypes.Surname)?.Value;

        var result = await _mediator.Send(new ProcessApprovalCommand
        {
            Id = id,
            Request = request,
            UserId = userId,
            UserName = userName
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}