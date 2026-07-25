using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/mfa")]
[ApiController]
[Authorize]
public class MfaController : ControllerBase
{
    private readonly IMfaService _mfa;
    private readonly ApplicationDbContext _db;

    public MfaController(IMfaService mfa, ApplicationDbContext db)
    {
        _mfa = mfa; _db = db;
    }

    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

    // GET /api/v1/mfa/setup — generate secret + QR code
    [HttpGet("setup")]
    public async Task<ActionResult<ApiResponse<object>>> Setup()
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == UserId);
        if (user == null) return NotFound(ApiResponse<object>.Fail("User not found"));
        if (user.TwoFactorEnabled)
            return BadRequest(ApiResponse<object>.Fail("MFA is already enabled"));

        var secret = _mfa.GenerateSecret();
        var qrCode = _mfa.GenerateQrCodeBase64(user.Email, secret);

        // Store secret temporarily (not yet confirmed)
        user.TwoFactorSecret = secret;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { secret, qrCode }, "Scan QR code with Google Authenticator"));
    }

    // POST /api/v1/mfa/verify — confirm TOTP code to enable MFA
    [HttpPost("verify")]
    public async Task<ActionResult<ApiResponse<object>>> Verify([FromBody] MfaVerifyRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == UserId);
        if (user == null) return NotFound(ApiResponse<object>.Fail("User not found"));
        if (string.IsNullOrEmpty(user.TwoFactorSecret))
            return BadRequest(ApiResponse<object>.Fail("MFA setup not initiated"));

        if (!_mfa.ValidateTotp(user.TwoFactorSecret, req.Code))
            return BadRequest(ApiResponse<object>.Fail("Invalid TOTP code"));

        user.TwoFactorEnabled = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var backupCodes = _mfa.GenerateBackupCodes();
        return Ok(ApiResponse<object>.Ok(new { backupCodes }, "MFA enabled successfully. Save your backup codes."));
    }

    // POST /api/v1/mfa/validate — validate TOTP during login
    [HttpPost("validate")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<bool>>> Validate([FromBody] MfaValidateRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLower());
        if (user == null || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
            return BadRequest(ApiResponse<bool>.Fail("MFA not configured for this user"));

        var valid = _mfa.ValidateTotp(user.TwoFactorSecret, req.Code);
        return Ok(ApiResponse<bool>.Ok(valid, valid ? "MFA validated" : "Invalid code"));
    }

    // DELETE /api/v1/mfa/disable
    [HttpDelete("disable")]
    public async Task<ActionResult<ApiResponse<bool>>> Disable([FromBody] MfaVerifyRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == UserId);
        if (user == null) return NotFound(ApiResponse<bool>.Fail("User not found"));
        if (!user.TwoFactorEnabled) return BadRequest(ApiResponse<bool>.Fail("MFA is not enabled"));

        if (!_mfa.ValidateTotp(user.TwoFactorSecret!, req.Code))
            return BadRequest(ApiResponse<bool>.Fail("Invalid TOTP code"));

        user.TwoFactorEnabled = false;
        user.TwoFactorSecret = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "MFA disabled"));
    }
}

public record MfaVerifyRequest(string Code);
public record MfaValidateRequest(string Email, string Code);
