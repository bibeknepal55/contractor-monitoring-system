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
    private readonly IPermissionResolver _permissionResolver;

    private const int MaxLoginAttempts = 5;
    private const int LockoutMinutes = 15;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordService passwordService,
        IPermissionResolver permissionResolver)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _permissionResolver = permissionResolver;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        var user = users.FirstOrDefault(u =>
            string.Equals(u.Email, command.Request.Email.Trim(), StringComparison.OrdinalIgnoreCase));

        if (user == null)
            return ApiResponse<AuthResponse>.Fail("Invalid email or password");

        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            var remainingMinutes = (int)(user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes;
            return ApiResponse<AuthResponse>.Fail($"Account is temporarily locked. Please try again in {remainingMinutes} minutes.");
        }

        if (!user.IsActive)
            return ApiResponse<AuthResponse>.Fail("Account is deactivated. Contact administrator.");

        if (!_passwordService.VerifyPassword(command.Request.Password, user.PasswordHash))
        {
            user.LoginAttempts++;
            if (user.LoginAttempts >= MaxLoginAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                user.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<AuthResponse>.Fail($"Account locked after {MaxLoginAttempts} failed attempts. Try again in {LockoutMinutes} minutes.");
            }
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            var remainingAttempts = MaxLoginAttempts - user.LoginAttempts;
            return ApiResponse<AuthResponse>.Fail($"Invalid email or password. {remainingAttempts} attempts remaining.");
        }

        user.LoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;

        var accessToken = await _jwtService.GenerateAccessToken(user);
        var refreshToken = await _jwtService.GenerateRefreshToken();
        var expiresAt = await _jwtService.GetTokenExpiryTime(accessToken);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Use centralized PermissionResolver
        var userRoles = await _permissionResolver.GetUserRolesAsync(user.Id);
        var userPermissions = await _permissionResolver.GetUserPermissionsAsync(user.Id);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
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
        }, "Login successful");
    }
}