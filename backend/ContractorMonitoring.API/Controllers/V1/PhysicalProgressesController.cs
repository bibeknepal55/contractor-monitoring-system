using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhysicalProgress;
using ContractorMonitoring.Application.Features.PhysicalProgress.Commands.Create;
using ContractorMonitoring.Application.Features.PhysicalProgress.Commands.Update;
using ContractorMonitoring.Application.Features.PhysicalProgress.Commands.Delete;
using ContractorMonitoring.Application.Features.PhysicalProgress.Queries.GetAll;
using ContractorMonitoring.Application.Features.PhysicalProgress.Queries.GetById;
using ContractorMonitoring.Domain.Constants;
using Asp.Versioning;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/physical-progress")]
[ApiController]
public class PhysicalProgressesController : ControllerBase
{
    private readonly IMediator _mediator;
    public PhysicalProgressesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = Permissions.PhysicalProgress.View)]
    public async Task<ActionResult<PagedResponse<PhysicalProgressDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = "asc")
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        return Ok(await _mediator.Send(new GetAllPhysicalProgressesQuery
        {
            Filter = new PaginationFilter { Page = page, PageSize = pageSize, Search = search, SortBy = sortBy, SortOrder = sortOrder },
            TenantId = tenantId
        }));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.PhysicalProgress.View)]
    public async Task<ActionResult<ApiResponse<PhysicalProgressDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPhysicalProgressByIdQuery { Id = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.PhysicalProgress.Create)]
    public async Task<ActionResult<ApiResponse<PhysicalProgressDto>>> Create([FromBody] CreatePhysicalProgressDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var result = await _mediator.Send(new CreatePhysicalProgressCommand { Request = request, UserId = userId, TenantId = tenantId });
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.PhysicalProgress.Update)]
    public async Task<ActionResult<ApiResponse<PhysicalProgressDto>>> Update(Guid id, [FromBody] UpdatePhysicalProgressDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var result = await _mediator.Send(new UpdatePhysicalProgressCommand { Id = id, Request = request, UserId = userId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.PhysicalProgress.Delete)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeletePhysicalProgressCommand { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

