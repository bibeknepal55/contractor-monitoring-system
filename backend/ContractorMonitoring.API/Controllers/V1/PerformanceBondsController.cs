using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PerformanceBond;
using ContractorMonitoring.Application.Features.PerformanceBond.Commands.Create;
using ContractorMonitoring.Application.Features.PerformanceBond.Commands.Update;
using ContractorMonitoring.Application.Features.PerformanceBond.Commands.Delete;
using ContractorMonitoring.Application.Features.PerformanceBond.Queries.GetAll;
using ContractorMonitoring.Application.Features.PerformanceBond.Queries.GetById;
using ContractorMonitoring.Domain.Constants;
using Asp.Versioning;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/performance-bonds")]
[ApiController]
public class PerformanceBondsController : ControllerBase
{
    private readonly IMediator _mediator;
    public PerformanceBondsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = Permissions.PerformanceBond.View)]
    public async Task<ActionResult<PagedResponse<PerformanceBondDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = "asc")
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        return Ok(await _mediator.Send(new GetAllPerformanceBondsQuery
        {
            Filter = new PaginationFilter { Page = page, PageSize = pageSize, Search = search, SortBy = sortBy, SortOrder = sortOrder },
            TenantId = tenantId
        }));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.PerformanceBond.View)]
    public async Task<ActionResult<ApiResponse<PerformanceBondDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPerformanceBondByIdQuery { Id = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.PerformanceBond.Create)]
    public async Task<ActionResult<ApiResponse<PerformanceBondDto>>> Create([FromBody] CreatePerformanceBondDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var result = await _mediator.Send(new CreatePerformanceBondCommand { Request = request, UserId = userId, TenantId = tenantId });
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.PerformanceBond.Update)]
    public async Task<ActionResult<ApiResponse<PerformanceBondDto>>> Update(Guid id, [FromBody] UpdatePerformanceBondDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var result = await _mediator.Send(new UpdatePerformanceBondCommand { Id = id, Request = request, UserId = userId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.PerformanceBond.Delete)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeletePerformanceBondCommand { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
