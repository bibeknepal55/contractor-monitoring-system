using Asp.Versioning;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.UserManagement;
using ContractorMonitoring.Application.Features.UserManagement.Commands.CreateUser;
using ContractorMonitoring.Application.Features.UserManagement.Commands.DeleteUser;
using ContractorMonitoring.Application.Features.UserManagement.Commands.UpdateRolePermissions;
using ContractorMonitoring.Application.Features.UserManagement.Commands.UpdateRoles;
using ContractorMonitoring.Application.Features.UserManagement.Commands.UpdateStatus;
using ContractorMonitoring.Application.Features.UserManagement.Queries.GetAll;
using ContractorMonitoring.Application.Features.UserManagement.Queries.GetRoles;
using ContractorMonitoring.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[ApiController]
public class UserManagementController : ControllerBase
{
    private readonly IMediator _mediator;
    public UserManagementController(IMediator mediator) => _mediator = mediator;

    // GET: api/v1/users
    [HttpGet]
    [Authorize(Policy = Permissions.UserManagement.View)]
    public async Task<ActionResult<PagedResponse<UserManagementDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = "asc",
        [FromQuery] string? isActive = null, [FromQuery] string? role = null)
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        return Ok(await _mediator.Send(new GetAllUsersQuery
        {
            Filter = new PaginationFilter { Page = page, PageSize = pageSize, Search = search, SortBy = sortBy, SortOrder = sortOrder },
            TenantId = tenantId,
            IsActiveFilter = isActive,
            RoleFilter = role
        }));
    }

    // PUT: api/v1/users/{id}/roles
    [HttpPut("{id:guid}/roles")]
    [Authorize(Policy = Permissions.UserManagement.Update)]
    public async Task<ActionResult<ApiResponse<UserManagementDto>>> UpdateRoles(Guid id, [FromBody] UpdateUserRolesDto request)
    {
        var updatedBy = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var result = await _mediator.Send(new UpdateUserRolesCommand { UserId = id, Request = request, UpdatedBy = updatedBy });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // PUT: api/v1/users/{id}/status
    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = Permissions.UserManagement.Update)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(Guid id, [FromBody] UpdateUserStatusDto request)
    {
        var result = await _mediator.Send(new UpdateUserStatusCommand { UserId = id, IsActive = request.IsActive });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET: api/v1/users/roles
    [HttpGet("roles")]
    [Authorize(Policy = Permissions.UserManagement.View)]
    public async Task<ActionResult<ApiResponse<List<RoleManagementDto>>>> GetRoles()
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());
        return Ok(await _mediator.Send(new GetRolesQuery { TenantId = tenantId }));
    }

    // PUT: api/v1/users/roles/{roleId}/permissions
    [HttpPut("roles/{roleId:guid}/permissions")]
    [Authorize(Policy = Permissions.UserManagement.AssignRole)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateRolePermissions(Guid roleId, [FromBody] UpdateRolePermissionsDto request)
    {
        var result = await _mediator.Send(new UpdateRolePermissionsCommand { RoleId = roleId, Permissions = request.Permissions });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // DELETE: api/v1/users/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.UserManagement.Delete)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(Guid id)
    {
        var deletedBy = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);

        var result = await _mediator.Send(new DeleteUserCommand
        {
            UserId = id,
            DeletedBy = deletedBy
        });

        return result.Success ? Ok(result) : BadRequest(result);
    }

    // POST: api/v1/users
    [HttpPost]
    [Authorize(Policy = Permissions.UserManagement.Create)]
    public async Task<ActionResult<ApiResponse<UserManagementDto>>> CreateUser([FromBody] CreateUserDto request)
    {
        var createdBy = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());

        var result = await _mediator.Send(new CreateUserCommand
        {
            Request = request,
            CreatedBy = createdBy,
            TenantId = tenantId
        });

        return result.Success ? CreatedAtAction(nameof(GetAll), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

}