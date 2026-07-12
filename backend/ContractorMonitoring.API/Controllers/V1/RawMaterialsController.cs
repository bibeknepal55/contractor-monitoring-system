using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RawMaterial;
using ContractorMonitoring.Application.Features.RawMaterial.Commands.Create;
using ContractorMonitoring.Application.Features.RawMaterial.Commands.Update;
using ContractorMonitoring.Application.Features.RawMaterial.Commands.Delete;
using ContractorMonitoring.Application.Features.RawMaterial.Queries.GetAll;
using ContractorMonitoring.Application.Features.RawMaterial.Queries.GetById;
using ContractorMonitoring.Domain.Constants;
using Asp.Versioning;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/raw-materials")]
[ApiController]
public class RawMaterialsController : ControllerBase
{
    private readonly IMediator _mediator;
    public RawMaterialsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = Permissions.RawMaterial.View)]
    public async Task<ActionResult<PagedResponse<RawMaterialDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = "asc")
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        return Ok(await _mediator.Send(new GetAllRawMaterialsQuery
        {
            Filter = new PaginationFilter { Page = page, PageSize = pageSize, Search = search, SortBy = sortBy, SortOrder = sortOrder },
            TenantId = tenantId
        }));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.RawMaterial.View)]
    public async Task<ActionResult<ApiResponse<RawMaterialDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetRawMaterialByIdQuery { Id = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.RawMaterial.Create)]
    public async Task<ActionResult<ApiResponse<RawMaterialDto>>> Create([FromBody] CreateRawMaterialDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var result = await _mediator.Send(new CreateRawMaterialCommand { Request = request, UserId = userId, TenantId = tenantId });
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.RawMaterial.Update)]
    public async Task<ActionResult<ApiResponse<RawMaterialDto>>> Update(Guid id, [FromBody] UpdateRawMaterialDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var result = await _mediator.Send(new UpdateRawMaterialCommand { Id = id, Request = request, UserId = userId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.RawMaterial.Delete)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteRawMaterialCommand { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
