using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Subcontractor;
using ContractorMonitoring.Application.Features.Subcontractor.Commands.Create;
using ContractorMonitoring.Application.Features.Subcontractor.Commands.Update;
using ContractorMonitoring.Application.Features.Subcontractor.Commands.Delete;
using ContractorMonitoring.Application.Features.Subcontractor.Queries.GetAll;
using ContractorMonitoring.Application.Features.Subcontractor.Queries.GetById;
using ContractorMonitoring.Domain.Constants;
using Asp.Versioning;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/subcontractors")]
[ApiController]
public class SubcontractorsController : ControllerBase
{
    private readonly IMediator _mediator;
    public SubcontractorsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = Permissions.Subcontractor.View)]
    public async Task<ActionResult<PagedResponse<SubcontractorDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = "asc")
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        return Ok(await _mediator.Send(new GetAllSubcontractorsQuery
        {
            Filter = new PaginationFilter { Page = page, PageSize = pageSize, Search = search, SortBy = sortBy, SortOrder = sortOrder },
            TenantId = tenantId
        }));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.Subcontractor.View)]
    public async Task<ActionResult<ApiResponse<SubcontractorDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetSubcontractorByIdQuery { Id = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Subcontractor.Create)]
    public async Task<ActionResult<ApiResponse<SubcontractorDto>>> Create([FromBody] CreateSubcontractorDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var result = await _mediator.Send(new CreateSubcontractorCommand { Request = request, UserId = userId, TenantId = tenantId });
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Subcontractor.Update)]
    public async Task<ActionResult<ApiResponse<SubcontractorDto>>> Update(Guid id, [FromBody] UpdateSubcontractorDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var result = await _mediator.Send(new UpdateSubcontractorCommand { Id = id, Request = request, UserId = userId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.Subcontractor.Delete)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteSubcontractorCommand { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
