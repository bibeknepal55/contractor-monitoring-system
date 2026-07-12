using Asp.Versioning;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ContractorOfficeDetail;
using ContractorMonitoring.Application.Features.ContractorOfficeDetail.Commands.Create;
using ContractorMonitoring.Application.Features.ContractorOfficeDetail.Commands.Delete;
using ContractorMonitoring.Application.Features.ContractorOfficeDetail.Commands.Update;
using ContractorMonitoring.Application.Features.ContractorOfficeDetail.Queries.GetAll;
using ContractorMonitoring.Application.Features.ContractorOfficeDetail.Queries.GetById;
using ContractorMonitoring.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contractors")]
[ApiController]
public class ContractorOfficeDetailsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContractorOfficeDetailsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.ContractorOfficeDetail.View)]
    public async Task<ActionResult<PagedResponse<ContractorOfficeDetailDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var query = new GetAllContractorOfficeDetailsQuery
        {
            Filter = new PaginationFilter { Page = page, PageSize = pageSize, Search = search, SortBy = sortBy, SortOrder = sortOrder },
            TenantId = tenantId
        };
        return Ok(await _mediator.Send(query));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.ContractorOfficeDetail.View)]
    public async Task<ActionResult<ApiResponse<ContractorOfficeDetailDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetContractorOfficeDetailByIdQuery { Id = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.ContractorOfficeDetail.Create)]
    public async Task<ActionResult<ApiResponse<ContractorOfficeDetailDto>>> Create([FromBody] CreateContractorOfficeDetailDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var result = await _mediator.Send(new CreateContractorOfficeDetailCommand { Request = request, UserId = userId, TenantId = tenantId });
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.ContractorOfficeDetail.Update)]
    public async Task<ActionResult<ApiResponse<ContractorOfficeDetailDto>>> Update(Guid id, [FromBody] UpdateContractorOfficeDetailDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var result = await _mediator.Send(new UpdateContractorOfficeDetailCommand { Id = id, Request = request, UserId = userId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.ContractorOfficeDetail.Delete)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteContractorOfficeDetailCommand { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}