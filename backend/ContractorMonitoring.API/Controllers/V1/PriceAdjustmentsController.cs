using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PriceAdjustment;
using ContractorMonitoring.Application.Features.PriceAdjustment.Commands.Create;
using ContractorMonitoring.Application.Features.PriceAdjustment.Commands.Update;
using ContractorMonitoring.Application.Features.PriceAdjustment.Commands.Delete;
using ContractorMonitoring.Application.Features.PriceAdjustment.Queries.GetAll;
using ContractorMonitoring.Application.Features.PriceAdjustment.Queries.GetById;
using ContractorMonitoring.Domain.Constants;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/price-adjustments")]
[ApiController]
public class PriceAdjustmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PriceAdjustmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.PriceAdjustment.View)]
    public async Task<ActionResult<PagedResponse<PriceAdjustmentDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        return Ok(await _mediator.Send(new GetAllPriceAdjustmentsQuery
        {
            Filter = new PaginationFilter
            {
                Page = page,
                PageSize = pageSize,
                Search = search,
                SortBy = sortBy,
                SortOrder = sortOrder
            },
            TenantId = tenantId
        }));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.PriceAdjustment.View)]
    public async Task<ActionResult<ApiResponse<PriceAdjustmentDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPriceAdjustmentByIdQuery { Id = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.PriceAdjustment.Create)]
    public async Task<ActionResult<ApiResponse<PriceAdjustmentDto>>> Create([FromBody] CreatePriceAdjustmentDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var userName = User.FindFirst(ClaimTypes.GivenName)?.Value + " " + User.FindFirst(ClaimTypes.Surname)?.Value;

        var result = await _mediator.Send(new CreatePriceAdjustmentCommand
        {
            Request = request,
            UserId = userId,
            TenantId = tenantId,
            UserName = userName
        });
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.PriceAdjustment.Update)]
    public async Task<ActionResult<ApiResponse<PriceAdjustmentDto>>> Update(Guid id, [FromBody] UpdatePriceAdjustmentDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var userName = User.FindFirst(ClaimTypes.GivenName)?.Value + " " + User.FindFirst(ClaimTypes.Surname)?.Value;

        var result = await _mediator.Send(new UpdatePriceAdjustmentCommand
        {
            Id = id,
            Request = request,
            UserId = userId,
            UserName = userName
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.PriceAdjustment.Delete)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeletePriceAdjustmentCommand { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}