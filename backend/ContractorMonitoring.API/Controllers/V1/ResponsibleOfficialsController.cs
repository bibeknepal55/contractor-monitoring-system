using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ResponsibleOfficial;
using ContractorMonitoring.Application.Features.ResponsibleOfficial.Commands.Create;
using ContractorMonitoring.Application.Features.ResponsibleOfficial.Commands.Update;
using ContractorMonitoring.Application.Features.ResponsibleOfficial.Commands.Delete;
using ContractorMonitoring.Application.Features.ResponsibleOfficial.Queries.GetAll;
using ContractorMonitoring.Application.Features.ResponsibleOfficial.Queries.GetById;
using ContractorMonitoring.Domain.Constants;
using Asp.Versioning;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/responsible-officials")]
[ApiController]
public class ResponsibleOfficialsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ResponsibleOfficialsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = Permissions.ResponsibleOfficial.View)]
    public async Task<ActionResult<PagedResponse<ResponsibleOfficialDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = "asc")
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        return Ok(await _mediator.Send(new GetAllResponsibleOfficialsQuery
        {
            Filter = new PaginationFilter { Page = page, PageSize = pageSize, Search = search, SortBy = sortBy, SortOrder = sortOrder },
            TenantId = tenantId
        }));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.ResponsibleOfficial.View)]
    public async Task<ActionResult<ApiResponse<ResponsibleOfficialDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetResponsibleOfficialByIdQuery { Id = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.ResponsibleOfficial.Create)]
    public async Task<ActionResult<ApiResponse<ResponsibleOfficialDto>>> Create([FromBody] CreateResponsibleOfficialDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var result = await _mediator.Send(new CreateResponsibleOfficialCommand { Request = request, UserId = userId, TenantId = tenantId });
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.ResponsibleOfficial.Update)]
    public async Task<ActionResult<ApiResponse<ResponsibleOfficialDto>>> Update(Guid id, [FromBody] UpdateResponsibleOfficialDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var result = await _mediator.Send(new UpdateResponsibleOfficialCommand { Id = id, Request = request, UserId = userId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.ResponsibleOfficial.Delete)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteResponsibleOfficialCommand { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
