using Asp.Versioning;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhotoMonitoring;
using ContractorMonitoring.Application.Features.PhotoMonitoring.Commands.Create;
using ContractorMonitoring.Application.Features.PhotoMonitoring.Commands.Delete;
using ContractorMonitoring.Application.Features.PhotoMonitoring.Commands.Update;
using ContractorMonitoring.Application.Features.PhotoMonitoring.Commands.Upload;
using ContractorMonitoring.Application.Features.PhotoMonitoring.Commands.UploadMultiple;
using ContractorMonitoring.Application.Features.PhotoMonitoring.Queries.Download;
using ContractorMonitoring.Application.Features.PhotoMonitoring.Queries.GetAll;
using ContractorMonitoring.Application.Features.PhotoMonitoring.Queries.GetById;
using ContractorMonitoring.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/photo-monitoring")]
[ApiController]
public class PhotoMonitoringsController : ControllerBase
{
    private readonly IMediator _mediator;
    public PhotoMonitoringsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = Permissions.PhotoMonitoring.View)]
    public async Task<ActionResult<PagedResponse<PhotoMonitoringDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = "asc")
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        return Ok(await _mediator.Send(new GetAllPhotoMonitoringsQuery
        {
            Filter = new PaginationFilter { Page = page, PageSize = pageSize, Search = search, SortBy = sortBy, SortOrder = sortOrder },
            TenantId = tenantId
        }));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.PhotoMonitoring.View)]
    public async Task<ActionResult<ApiResponse<PhotoMonitoringDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPhotoMonitoringByIdQuery { Id = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.PhotoMonitoring.Create)]
    public async Task<ActionResult<ApiResponse<PhotoMonitoringDto>>> Create([FromBody] CreatePhotoMonitoringDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var result = await _mediator.Send(new CreatePhotoMonitoringCommand { Request = request, UserId = userId, TenantId = tenantId });
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.PhotoMonitoring.Update)]
    public async Task<ActionResult<ApiResponse<PhotoMonitoringDto>>> Update(Guid id, [FromBody] UpdatePhotoMonitoringDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var result = await _mediator.Send(new UpdatePhotoMonitoringCommand { Id = id, Request = request, UserId = userId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.PhotoMonitoring.Delete)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeletePhotoMonitoringCommand { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }


// POST: api/v1/photo-monitoring/upload
[HttpPost("upload")]
    [Authorize(Policy = Permissions.PhotoMonitoring.Create)]
    public async Task<ActionResult<ApiResponse<PhotoMonitoringDto>>> Upload(
    [FromForm] IFormFile file,
    [FromForm] Guid projectId,
    [FromForm] string title,
    [FromForm] string description,
    [FromForm] DateTime photoDate,
    [FromForm] string location,
    [FromForm] string? direction = null,
    [FromForm] string? photoType = null,
    [FromForm] string? tags = null)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var userName = User.FindFirst(ClaimTypes.GivenName)?.Value + " " + User.FindFirst(ClaimTypes.Surname)?.Value;

        var request = new UploadPhotoDto
        {
            ProjectId = projectId,
            Title = title,
            Description = description,
            PhotoDate = photoDate,
            Location = location,
            Direction = direction,
            PhotoType = photoType,
            Tags = tags
        };

        var result = await _mediator.Send(new UploadPhotoCommand
        {
            File = file,
            Request = request,
            UserId = userId,
            TenantId = tenantId,
            UserName = userName
        });

        return result.Success ? Ok(result) : BadRequest(result);
    }

    // POST: api/v1/photo-monitoring/upload-multiple
    [HttpPost("upload-multiple")]
    [Authorize(Policy = Permissions.PhotoMonitoring.Create)]
    public async Task<ActionResult<ApiResponse<List<PhotoMonitoringDto>>>> UploadMultiple(
        [FromForm] List<IFormFile> files,
        [FromForm] Guid projectId,
        [FromForm] string title,
        [FromForm] string description,
        [FromForm] DateTime photoDate,
        [FromForm] string location,
        [FromForm] string? direction = null,
        [FromForm] string? photoType = null,
        [FromForm] string? tags = null)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        var userName = User.FindFirst(ClaimTypes.GivenName)?.Value + " " + User.FindFirst(ClaimTypes.Surname)?.Value;

        var request = new UploadPhotoDto
        {
            ProjectId = projectId,
            Title = title,
            Description = description,
            PhotoDate = photoDate,
            Location = location,
            Direction = direction,
            PhotoType = photoType,
            Tags = tags
        };

        var result = await _mediator.Send(new UploadMultiplePhotosCommand
        {
            Files = files,
            Request = request,
            UserId = userId,
            TenantId = tenantId,
            UserName = userName
        });

        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET: api/v1/photo-monitoring/{id}/download
    [HttpGet("{id:guid}/download")]
    [Authorize(Policy = Permissions.PhotoMonitoring.View)]
    public async Task<IActionResult> Download(Guid id)
    {
        var result = await _mediator.Send(new DownloadPhotoQuery { Id = id });

        if (!result.Success || result.Data == null)
            return NotFound(result);

        return File(result.Data.FileBytes, result.Data.ContentType, result.Data.FileName);
    }
}