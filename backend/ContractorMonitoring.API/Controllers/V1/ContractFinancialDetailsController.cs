using Asp.Versioning;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ContractFinancialDetail;
using ContractorMonitoring.Application.Features.ContractFinancialDetail.Commands.Create;
using ContractorMonitoring.Application.Features.ContractFinancialDetail.Commands.Delete;
using ContractorMonitoring.Application.Features.ContractFinancialDetail.Commands.Update;
using ContractorMonitoring.Application.Features.ContractFinancialDetail.Queries.GetAll;
using ContractorMonitoring.Application.Features.ContractFinancialDetail.Queries.GetById;
using ContractorMonitoring.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contract-financials")]
[ApiController]
public class ContractFinancialDetailsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ContractFinancialDetailsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = Permissions.ContractFinancialDetail.View)]
    public async Task<ActionResult<PagedResponse<ContractFinancialDetailDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = "asc")
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        return Ok(await _mediator.Send(new GetAllContractFinancialDetailsQuery
        {
            Filter = new PaginationFilter { Page = page, PageSize = pageSize, Search = search, SortBy = sortBy, SortOrder = sortOrder },
            TenantId = tenantId
        }));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.ContractFinancialDetail.View)]
    public async Task<ActionResult<ApiResponse<ContractFinancialDetailDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetContractFinancialDetailByIdQuery { Id = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.ContractFinancialDetail.Create)]
    public async Task<ActionResult<ApiResponse<ContractFinancialDetailDto>>> Create([FromBody] CreateContractFinancialDetailDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var result = await _mediator.Send(new CreateContractFinancialDetailCommand { Request = request, UserId = userId, TenantId = tenantId });
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.ContractFinancialDetail.Update)]
    public async Task<ActionResult<ApiResponse<ContractFinancialDetailDto>>> Update(Guid id, [FromBody] UpdateContractFinancialDetailDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var result = await _mediator.Send(new UpdateContractFinancialDetailCommand { Id = id, Request = request, UserId = userId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.ContractFinancialDetail.Delete)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteContractFinancialDetailCommand { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}