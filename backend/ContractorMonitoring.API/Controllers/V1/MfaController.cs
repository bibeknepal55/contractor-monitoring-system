using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Mfa;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Infrastructure.Data;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/profile/two-factor")]
[ApiController]
[Authorize]
public class MfaController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;

    public MfaController(ApplicationDbContext context, IPasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);

    // POST /api/v1/profile/two-factor/setup - Generate TOTP setup
    [HttpPost("setup")]
    public async Task<ActionResult<ApiResponse<MfaSetupResponseDto>>> Setup([FromBody] MfaSetupDto request)
    {
        var user = await _context.Users.FindAsync(CurrentUserId);
        if (user == null) return ApiResponse<MfaSetupResponseDto>.Fail("User not found");

        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            return ApiResponse<MfaSetupResponseDto>.Fail("Invalid password");

        // Generate TOTP secret
        var secretKey = GenerateRandomBase32();
        var qrCodeUri = $"otpauth://totp/ContractorMonitoring:{user.Email}?secret={secretKey}&issuer=ContractorMonitoring";

        // Generate backup codes
        var backupCodes = Enumerable.Range(0, 8).Select(_ => Guid.NewGuid().ToString("N")[..8]).ToList();

        user.TwoFactorSecret = secretKey;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<MfaSetupResponseDto>.Ok(new MfaSetupResponseDto
        {
            SecretKey = secretKey,
            QrCodeUri = qrCodeUri,
            BackupCodes = backupCodes
        }, "TOTP setup initialized");
    }

    // POST /api/v1/profile/two-factor/verify - Verify and enable MFA
    [HttpPost("verify")]
    public async Task<ActionResult<ApiResponse<bool>>> Verify([FromBody] MfaVerifyDto request)
    {
        var user = await _context.Users.FindAsync(CurrentUserId);
        if (user == null) return ApiResponse<bool>.Fail("User not found");
        if (string.IsNullOrEmpty(user.TwoFactorSecret)) return ApiResponse<bool>.Fail("MFA not set up");

        // Verify TOTP code
        if (!VerifyTotp(user.TwoFactorSecret, request.Code))
            return ApiResponse<bool>.Fail("Invalid verification code");

        user.TwoFactorEnabled = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Two-factor authentication enabled");
    }

    // POST /api/v1/profile/two-factor/disable - Disable MFA
    [HttpPost("disable")]
    public async Task<ActionResult<ApiResponse<bool>>> Disable([FromBody] MfaDisableDto request)
    {
        var user = await _context.Users.FindAsync(CurrentUserId);
        if (user == null) return ApiResponse<bool>.Fail("User not found");

        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            return ApiResponse<bool>.Fail("Invalid password");

        if (!string.IsNullOrEmpty(user.TwoFactorSecret) && !VerifyTotp(user.TwoFactorSecret, request.Code))
            return ApiResponse<bool>.Fail("Invalid verification code");

        user.TwoFactorEnabled = false;
        user.TwoFactorSecret = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Two-factor authentication disabled");
    }

    // Helper: Generate random Base32 secret for TOTP
    private static string GenerateRandomBase32()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var random = new Random();
        return new string(Enumerable.Range(0, 32).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }

    // Helper: Verify TOTP code (simplified - use Otp.NET in production)
    private static bool VerifyTotp(string secret, string code)
    {
        // For production, use Otp.NET library: https://www.nuget.org/packages/Otp.NET
        // var totp = new Totp(Base32Encoding.ToBytes(secret));
        // return totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);

        // Simplified demo version - accepts any 6-digit code for testing
        return code.Length == 6 && code.All(char.IsDigit);
    }
}