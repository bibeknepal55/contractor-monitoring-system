using Asp.Versioning;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Project;
using ContractorMonitoring.Application.Features.Project.Commands.Create;
using ContractorMonitoring.Application.Features.Project.Commands.Delete;
using ContractorMonitoring.Application.Features.Project.Commands.Update;
using ContractorMonitoring.Application.Features.Project.Queries.GetAll;
using ContractorMonitoring.Application.Features.Project.Queries.GetById;
using ContractorMonitoring.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContractorMonitoring.API.Controllers.V1;

// Projects management controller
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects")]
[ApiController]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET: api/v1/projects
    [HttpGet]
    [Authorize(Policy = Permissions.Project.View)]
    public async Task<ActionResult<PagedResponse<ProjectDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());

        var filter = new PaginationFilter
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            SortBy = sortBy,
            SortOrder = sortOrder
        };

        var query = new GetAllProjectsQuery
        {
            Filter = filter,
            TenantId = tenantId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // GET: api/v1/projects/{id}
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.Project.View)]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> GetById(Guid id)
    {
        var query = new GetProjectByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        return result.Success ? Ok(result) : NotFound(result);
    }

    // POST: api/v1/projects
    [HttpPost]
    [Authorize(Policy = Permissions.Project.Create)]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> Create([FromBody] CreateProjectDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());

        var command = new CreateProjectCommand
        {
            Request = request,
            UserId = userId,
            TenantId = tenantId
        };

        var result = await _mediator.Send(command);

        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    // PUT: api/v1/projects/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Project.Update)]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> Update(Guid id, [FromBody] UpdateProjectDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);

        var command = new UpdateProjectCommand
        {
            Id = id,
            Request = request,
            UserId = userId
        };

        var result = await _mediator.Send(command);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    // DELETE: api/v1/projects/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.Project.Delete)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var command = new DeleteProjectCommand { Id = id };
        var result = await _mediator.Send(command);

        return result.Success ? Ok(result) : BadRequest(result);
    }
}