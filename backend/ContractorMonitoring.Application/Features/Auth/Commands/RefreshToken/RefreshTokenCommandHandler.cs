using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Auth;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPermissionResolver _permissionResolver;

    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPermissionResolver permissionResolver)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _permissionResolver = permissionResolver;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _jwtService.ValidateToken(command.AccessToken, validateLifetime: false);

        if (!isValid)
            return ApiResponse<AuthResponse>.Fail("Invalid access token");

        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user == null)
            return ApiResponse<AuthResponse>.Fail("User not found");

        // Token reuse detection: if the submitted token doesn't match the stored one
        // but a family exists, this is a replay attack — invalidate all tokens for this user
        if (user.RefreshToken != command.RefreshToken)
        {
            if (!string.IsNullOrEmpty(user.RefreshTokenFamily))
            {
                // Potential token theft — revoke entire family
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                user.RefreshTokenFamily = null;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }
            return ApiResponse<AuthResponse>.Fail("Invalid refresh token");
        }

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return ApiResponse<AuthResponse>.Fail("Refresh token has expired. Please login again.");

        // Rotate: issue new token, revoke old one, preserve family
        var newAccessToken = await _jwtService.GenerateAccessToken(user);
        var newRefreshToken = await _jwtService.GenerateRefreshToken();
        var expiresAt = await _jwtService.GetTokenExpiryTime(newAccessToken);

        // Preserve family (set on first login if null)
        user.RefreshTokenFamily ??= Guid.NewGuid().ToString();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var userRoles = await _permissionResolver.GetUserRolesAsync(user.Id);
        var userPermissions = await _permissionResolver.GetUserPermissionsAsync(user.Id);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
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
        }, "Token refreshed successfully");
    }
}
