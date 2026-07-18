using System.Security.Claims;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RoleManagement;
using ContractorMonitoring.Application.Features.RoleManagement.Queries.GetAllRoles;
using ContractorMonitoring.Application.Features.RoleManagement.Queries.GetRoleById;
using ContractorMonitoring.Application.Features.RoleManagement.Queries.GetModulePermissions;
using ContractorMonitoring.Application.Features.RoleManagement.Commands.CreateRole;
using ContractorMonitoring.Application.Features.RoleManagement.Commands.UpdateRole;
using ContractorMonitoring.Application.Features.RoleManagement.Commands.DeleteRole;
using ContractorMonitoring.Domain.Constants;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/roles")]
[ApiController]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator) => _mediator = mediator;

    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
    private string UserName => $"{User.FindFirst(ClaimTypes.GivenName)?.Value} {User.FindFirst(ClaimTypes.Surname)?.Value}".Trim();
    private bool IsSuperAdmin => User.Claims.Any(c => c.Value == "SuperAdmin");
    private bool IsAdmin => User.Claims.Any(c => c.Value == "Admin");

    // GET /api/v1/roles - List all roles with user count
    [HttpGet]
    [Authorize(Policy = Permissions.UserManagement.View)]
    public async Task<ActionResult<ApiResponse<List<RoleDto>>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllRolesQuery());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET /api/v1/roles/{id} - Get single role with permissions
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.UserManagement.View)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetRoleByIdQuery { RoleId = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET /api/v1/roles/modules/permissions - Get permission tree for role dialog
    [HttpGet("modules/permissions")]
    [Authorize(Policy = Permissions.UserManagement.View)]
    public async Task<ActionResult<ApiResponse<List<ModulePermissionDto>>>> GetModulePermissions()
    {
        var result = await _mediator.Send(new GetModulePermissionsQuery { IsSuperAdmin = IsSuperAdmin });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // POST /api/v1/roles - Create custom role
    [HttpPost]
    [Authorize(Policy = Permissions.UserManagement.Create)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create([FromBody] CreateRoleDto request)
    {
        var result = await _mediator.Send(new CreateRoleCommand
        {
            Name = request.Name,
            Description = request.Description,
            PermissionIds = request.PermissionIds,
            CreatedBy = UserName,
            IsSuperAdmin = IsSuperAdmin
        });
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    // PUT /api/v1/roles/{id} - Update role
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.UserManagement.Update)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Update(Guid id, [FromBody] UpdateRoleDto request)
    {
        var result = await _mediator.Send(new UpdateRoleCommand
        {
            RoleId = id,
            Name = request.Name,
            Description = request.Description,
            PermissionIds = request.PermissionIds,
            IsSuperAdmin = IsSuperAdmin
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // DELETE /api/v1/roles/{id} - Delete custom role only
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.UserManagement.Delete)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteRoleCommand { RoleId = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}