using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.AdvancePaymentGuarantee;
using ContractorMonitoring.Application.Features.AdvancePaymentGuarantee.Commands.Create;
using ContractorMonitoring.Application.Features.AdvancePaymentGuarantee.Commands.Update;
using ContractorMonitoring.Application.Features.AdvancePaymentGuarantee.Commands.Delete;
using ContractorMonitoring.Application.Features.AdvancePaymentGuarantee.Queries.GetAll;
using ContractorMonitoring.Application.Features.AdvancePaymentGuarantee.Queries.GetById;
using ContractorMonitoring.Domain.Constants;
using Asp.Versioning;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/advance-payment-guarantees")]
[ApiController]
public class AdvancePaymentGuaranteesController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdvancePaymentGuaranteesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = Permissions.AdvancePaymentGuarantee.View)]
    public async Task<ActionResult<PagedResponse<AdvancePaymentGuaranteeDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = "asc")
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        return Ok(await _mediator.Send(new GetAllAdvancePaymentGuaranteesQuery
        {
            Filter = new PaginationFilter { Page = page, PageSize = pageSize, Search = search, SortBy = sortBy, SortOrder = sortOrder },
            TenantId = tenantId
        }));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.AdvancePaymentGuarantee.View)]
    public async Task<ActionResult<ApiResponse<AdvancePaymentGuaranteeDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetAdvancePaymentGuaranteeByIdQuery { Id = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.AdvancePaymentGuarantee.Create)]
    public async Task<ActionResult<ApiResponse<AdvancePaymentGuaranteeDto>>> Create([FromBody] CreateAdvancePaymentGuaranteeDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var result = await _mediator.Send(new CreateAdvancePaymentGuaranteeCommand { Request = request, UserId = userId, TenantId = tenantId });
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.AdvancePaymentGuarantee.Update)]
    public async Task<ActionResult<ApiResponse<AdvancePaymentGuaranteeDto>>> Update(Guid id, [FromBody] UpdateAdvancePaymentGuaranteeDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var result = await _mediator.Send(new UpdateAdvancePaymentGuaranteeCommand { Id = id, Request = request, UserId = userId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.AdvancePaymentGuarantee.Delete)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteAdvancePaymentGuaranteeCommand { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
