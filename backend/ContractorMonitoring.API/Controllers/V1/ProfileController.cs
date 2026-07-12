using System.Security.Claims;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Profile;
using ContractorMonitoring.Application.DTOs.Auth;
using ContractorMonitoring.Application.Features.Profile.Queries.GetProfile;
using ContractorMonitoring.Application.Features.Profile.Queries.GetPicture;
using ContractorMonitoring.Application.Features.Profile.Commands.UpdateProfile;
using ContractorMonitoring.Application.Features.Profile.Commands.UpdatePreferences;
using ContractorMonitoring.Application.Features.Profile.Commands.UploadPicture;
using ContractorMonitoring.Application.Features.Profile.Commands.DeletePicture;
using ContractorMonitoring.Application.Features.Profile.Commands.SecurityQuestion;
using ContractorMonitoring.Application.Features.Profile.Commands.TwoFactor;
using ContractorMonitoring.Application.Features.Profile.Commands.RevokeSession;
using ContractorMonitoring.Application.Features.Profile.Queries.GetSessions;
using ContractorMonitoring.Application.Features.Profile.Queries.GetActivities;
using ContractorMonitoring.Application.Features.Auth.Commands.ChangePassword;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/profile")]
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
    private string IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    // GET: api/v1/profile
    [HttpGet]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> GetProfile()
    {
        var result = await _mediator.Send(new GetProfileQuery { UserId = UserId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // PUT: api/v1/profile
    [HttpPut]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> UpdateProfile([FromBody] UpdateProfileDto request)
    {
        var result = await _mediator.Send(new UpdateProfileCommand
        {
            UserId = UserId,
            Request = request,
            IpAddress = IpAddress,
            DeviceInfo = Request.Headers.UserAgent.ToString()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // PUT: api/v1/profile/preferences
    [HttpPut("preferences")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdatePreferences([FromBody] UpdatePreferencesDto request)
    {
        var result = await _mediator.Send(new UpdatePreferencesCommand { UserId = UserId, Request = request });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // PUT: api/v1/profile/password
    [HttpPut("password")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await _mediator.Send(new ChangePasswordCommand { UserId = UserId, Request = request });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // POST: api/v1/profile/picture
    [HttpPost("picture")]
    public async Task<ActionResult<ApiResponse<ProfilePictureDto>>> UploadPicture(IFormFile file)
    {
        var result = await _mediator.Send(new UploadPictureCommand { UserId = UserId, File = file });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // DELETE: api/v1/profile/picture
    [HttpDelete("picture")]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePicture()
    {
        var result = await _mediator.Send(new DeletePictureCommand { UserId = UserId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET: api/v1/profile/picture
    [HttpGet("picture")]
    public async Task<IActionResult> GetPicture()
    {
        var result = await _mediator.Send(new GetPictureQuery { UserId = UserId });
        if (!result.Success || result.Data?.FileBytes == null || result.Data.FileBytes.Length == 0)
            return NotFound(result);
        return File(result.Data.FileBytes, result.Data.ContentType, result.Data.FileName);
    }

    // PUT: api/v1/profile/security-question
    [HttpPut("security-question")]
    public async Task<ActionResult<ApiResponse<bool>>> SetupSecurityQuestion([FromBody] SecurityQuestionDto request)
    {
        var result = await _mediator.Send(new SecurityQuestionCommand { UserId = UserId, Request = request });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // PUT: api/v1/profile/two-factor
    [HttpPut("two-factor")]
    public async Task<ActionResult<ApiResponse<bool>>> SetupTwoFactor([FromBody] TwoFactorSetupDto request)
    {
        var result = await _mediator.Send(new TwoFactorCommand { UserId = UserId, Request = request });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET: api/v1/profile/sessions
    [HttpGet("sessions")]
    public async Task<ActionResult<ApiResponse<List<SessionDto>>>> GetSessions()
    {
        var result = await _mediator.Send(new GetSessionsQuery { UserId = UserId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // DELETE: api/v1/profile/sessions/{id}
    [HttpDelete("sessions/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> RevokeSession(Guid id)
    {
        var result = await _mediator.Send(new RevokeSessionCommand { UserId = UserId, SessionId = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET: api/v1/profile/activities
    [HttpGet("activities")]
    public async Task<ActionResult<ApiResponse<List<ActivityDto>>>> GetActivities()
    {
        var result = await _mediator.Send(new GetActivitiesQuery { UserId = UserId });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}