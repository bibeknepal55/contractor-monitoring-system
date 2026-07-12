using Asp.Versioning;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Auth;
using ContractorMonitoring.Application.Features.Auth.Commands.ChangePassword;
using ContractorMonitoring.Application.Features.Auth.Commands.Login;
using ContractorMonitoring.Application.Features.Auth.Commands.Logout;
using ContractorMonitoring.Application.Features.Auth.Commands.RefreshToken;
using ContractorMonitoring.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContractorMonitoring.API.Controllers.V1;

// Authentication controller
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // POST: api/v1/auth/register
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand { Request = request };
        var result = await _mediator.Send(command);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    // POST: api/v1/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand { Request = request };
        var result = await _mediator.Send(command);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    // POST: api/v1/auth/refresh-token
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    // POST: api/v1/auth/logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> Logout()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var command = new LogoutCommand { UserId = userId };
        var result = await _mediator.Send(command);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    // POST: api/v1/auth/change-password
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var command = new ChangePasswordCommand { UserId = userId, Request = request };
        var result = await _mediator.Send(command);

        return result.Success ? Ok(result) : BadRequest(result);
    }
}