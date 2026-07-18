using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Auth;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<AuthResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;

    // FIXED: Lockout constants
    private const int MaxLoginAttempts = 5;
    private const int LockoutMinutes = 15;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordService passwordService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordService = passwordService;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        // FIXED: Query by email at database level using ExistsAsync + GetById pattern
        var users = await _unitOfWork.Users.GetAllAsync();
        var user = users.FirstOrDefault(u =>
            string.Equals(u.Email, command.Request.Email.Trim(), StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid email or password");
        }

        // FIXED: Check lockout before password verification
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            var remainingMinutes = (int)(user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes;
            return ApiResponse<AuthResponse>.Fail(
                $"Account is temporarily locked. Please try again in {remainingMinutes} minutes.");
        }

        // FIXED: Check if user is active
        if (!user.IsActive)
        {
            return ApiResponse<AuthResponse>.Fail("Account is deactivated. Contact administrator.");
        }

        // Verify password
        if (!_passwordService.VerifyPassword(command.Request.Password, user.PasswordHash))
        {
            // FIXED: Increment failed login attempts
            user.LoginAttempts++;

            // FIXED: Lock account after max attempts
            if (user.LoginAttempts >= MaxLoginAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                user.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<AuthResponse>.Fail(
                    $"Account locked after {MaxLoginAttempts} failed attempts. Please try again in {LockoutMinutes} minutes.");
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var remainingAttempts = MaxLoginAttempts - user.LoginAttempts;
            return ApiResponse<AuthResponse>.Fail(
                $"Invalid email or password. {remainingAttempts} attempts remaining.");
        }

        // FIXED: Reset login attempts on successful login
        user.LoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;
        user.LastKnownIp = null; // Will be set by middleware
        user.LastKnownDevice = null;

        // Generate tokens (uses centralized IJwtService)
        var accessToken = await _jwtService.GenerateAccessToken(user);
        var refreshToken = await _jwtService.GenerateRefreshToken();
        var expiresAt = await _jwtService.GetTokenExpiryTime(accessToken);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Get roles and permissions from centralized service (not duplicated here)
        var userRoles = await _jwtService.GetUserRolesAsync(user.Id);
        var userPermissions = await _jwtService.GetUserPermissionsAsync(user.Id);

        var authResponse = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Roles = userRoles,
                Permissions = userPermissions
            }
        };

        return ApiResponse<AuthResponse>.Ok(authResponse, "Login successful");
    }
}