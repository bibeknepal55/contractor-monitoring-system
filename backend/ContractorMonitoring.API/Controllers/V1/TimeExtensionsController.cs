using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.TimeExtension;
using ContractorMonitoring.Application.Features.TimeExtension.Commands.Create;
using ContractorMonitoring.Application.Features.TimeExtension.Commands.Update;
using ContractorMonitoring.Application.Features.TimeExtension.Commands.Delete;
using ContractorMonitoring.Application.Features.TimeExtension.Queries.GetAll;
using ContractorMonitoring.Application.Features.TimeExtension.Queries.GetById;
using ContractorMonitoring.Domain.Constants;
using Asp.Versioning;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/time-extensions")]
[ApiController]
public class TimeExtensionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public TimeExtensionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = Permissions.TimeExtension.View)]
    public async Task<ActionResult<PagedResponse<TimeExtensionDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = "asc")
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        return Ok(await _mediator.Send(new GetAllTimeExtensionsQuery
        {
            Filter = new PaginationFilter { Page = page, PageSize = pageSize, Search = search, SortBy = sortBy, SortOrder = sortOrder },
            TenantId = tenantId
        }));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.TimeExtension.View)]
    public async Task<ActionResult<ApiResponse<TimeExtensionDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTimeExtensionByIdQuery { Id = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.TimeExtension.Create)]
    public async Task<ActionResult<ApiResponse<TimeExtensionDto>>> Create([FromBody] CreateTimeExtensionDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var result = await _mediator.Send(new CreateTimeExtensionCommand { Request = request, UserId = userId, TenantId = tenantId });
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.TimeExtension.Update)]
    public async Task<ActionResult<ApiResponse<TimeExtensionDto>>> Update(Guid id, [FromBody] UpdateTimeExtensionDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var result = await _mediator.Send(new UpdateTimeExtensionCommand { Id = id, Request = request, UserId = userId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.TimeExtension.Delete)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteTimeExtensionCommand { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
