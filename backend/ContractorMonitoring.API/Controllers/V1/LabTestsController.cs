using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.LabTest;
using ContractorMonitoring.Application.Features.LabTest.Commands.Create;
using ContractorMonitoring.Application.Features.LabTest.Commands.Update;
using ContractorMonitoring.Application.Features.LabTest.Commands.Delete;
using ContractorMonitoring.Application.Features.LabTest.Queries.GetAll;
using ContractorMonitoring.Application.Features.LabTest.Queries.GetById;
using ContractorMonitoring.Domain.Constants;
using Asp.Versioning;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/lab-tests")]
[ApiController]
public class LabTestsController : ControllerBase
{
    private readonly IMediator _mediator;
    public LabTestsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = Permissions.LabTest.View)]
    public async Task<ActionResult<PagedResponse<LabTestDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = "asc")
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        return Ok(await _mediator.Send(new GetAllLabTestsQuery
        {
            Filter = new PaginationFilter { Page = page, PageSize = pageSize, Search = search, SortBy = sortBy, SortOrder = sortOrder },
            TenantId = tenantId
        }));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.LabTest.View)]
    public async Task<ActionResult<ApiResponse<LabTestDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetLabTestByIdQuery { Id = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.LabTest.Create)]
    public async Task<ActionResult<ApiResponse<LabTestDto>>> Create([FromBody] CreateLabTestDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var result = await _mediator.Send(new CreateLabTestCommand { Request = request, UserId = userId, TenantId = tenantId });
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.LabTest.Update)]
    public async Task<ActionResult<ApiResponse<LabTestDto>>> Update(Guid id, [FromBody] UpdateLabTestDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var result = await _mediator.Send(new UpdateLabTestCommand { Id = id, Request = request, UserId = userId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.LabTest.Delete)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteLabTestCommand { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
