using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Auth;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Auth.Commands.RefreshToken;

// Handler for refreshing JWT tokens
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        // Validate access token to get user ID
        var (isValid, userId) = await _jwtService.ValidateToken(command.AccessToken);

        if (!isValid)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid access token");
        }

        // Get user
        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user == null || user.RefreshToken != command.RefreshToken)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid refresh token");
        }

        // Check if refresh token is expired
        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return ApiResponse<AuthResponse>.Fail("Refresh token has expired. Please login again.");
        }

        // Generate new tokens
        var newAccessToken = await _jwtService.GenerateAccessToken(user);
        var newRefreshToken = await _jwtService.GenerateRefreshToken();
        var expiresAt = await _jwtService.GetTokenExpiryTime(newAccessToken);

        // Update user with new refresh token
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Get user roles and permissions
        var userRoles = await GetUserRoles(user.Id);
        var userPermissions = await GetUserPermissions(user.Id);

        var authResponse = new AuthResponse
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
        };

        return ApiResponse<AuthResponse>.Ok(authResponse, "Token refreshed successfully");
    }

    private async Task<List<string>> GetUserRoles(Guid userId)
    {
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
        var roles = await _unitOfWork.Roles.GetAllAsync();

        return (from ur in userRoles
                join r in roles on ur.RoleId equals r.Id
                where ur.UserId == userId && !ur.IsDeleted && !r.IsDeleted
                select r.Name).ToList();
    }

    private async Task<List<string>> GetUserPermissions(Guid userId)
    {
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
        var rolePermissions = await _unitOfWork.RolePermissions.GetAllAsync();
        var permissions = await _unitOfWork.Permissions.GetAllAsync();

        return (from ur in userRoles
                join rp in rolePermissions on ur.RoleId equals rp.RoleId
                join p in permissions on rp.PermissionId equals p.Id
                where ur.UserId == userId && !ur.IsDeleted && !rp.IsDeleted && !p.IsDeleted
                select p.Name).Distinct().ToList();
    }
}