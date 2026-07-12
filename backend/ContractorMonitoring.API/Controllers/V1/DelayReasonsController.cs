using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.DelayReason;
using ContractorMonitoring.Application.Features.DelayReason.Commands.Create;
using ContractorMonitoring.Application.Features.DelayReason.Commands.Update;
using ContractorMonitoring.Application.Features.DelayReason.Commands.Delete;
using ContractorMonitoring.Application.Features.DelayReason.Queries.GetAll;
using ContractorMonitoring.Application.Features.DelayReason.Queries.GetById;
using ContractorMonitoring.Domain.Constants;
using Asp.Versioning;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/delay-reasons")]
[ApiController]
public class DelayReasonsController : ControllerBase
{
    private readonly IMediator _mediator;
    public DelayReasonsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = Permissions.DelayReason.View)]
    public async Task<ActionResult<PagedResponse<DelayReasonDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = "asc")
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        return Ok(await _mediator.Send(new GetAllDelayReasonsQuery
        {
            Filter = new PaginationFilter { Page = page, PageSize = pageSize, Search = search, SortBy = sortBy, SortOrder = sortOrder },
            TenantId = tenantId
        }));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.DelayReason.View)]
    public async Task<ActionResult<ApiResponse<DelayReasonDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetDelayReasonByIdQuery { Id = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.DelayReason.Create)]
    public async Task<ActionResult<ApiResponse<DelayReasonDto>>> Create([FromBody] CreateDelayReasonDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var result = await _mediator.Send(new CreateDelayReasonCommand { Request = request, UserId = userId, TenantId = tenantId });
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.DelayReason.Update)]
    public async Task<ActionResult<ApiResponse<DelayReasonDto>>> Update(Guid id, [FromBody] UpdateDelayReasonDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var result = await _mediator.Send(new UpdateDelayReasonCommand { Id = id, Request = request, UserId = userId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.DelayReason.Delete)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteDelayReasonCommand { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
