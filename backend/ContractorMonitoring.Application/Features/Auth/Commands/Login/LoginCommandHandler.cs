using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Auth;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Auth.Commands.Login;

// Handler for user login
public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<AuthResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;

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
        // Find user by email
        var users = await _unitOfWork.Users.GetAllAsync();
        var user = users.FirstOrDefault(u => u.Email == command.Request.Email.ToLower().Trim());

        if (user == null)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid email or password");
        }

        // Verify password
        if (!_passwordService.VerifyPassword(command.Request.Password, user.PasswordHash))
        {
            return ApiResponse<AuthResponse>.Fail("Invalid email or password");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return ApiResponse<AuthResponse>.Fail("Account is deactivated. Contact administrator.");
        }

        // Generate tokens
        var accessToken = await _jwtService.GenerateAccessToken(user);
        var refreshToken = await _jwtService.GenerateRefreshToken();
        var expiresAt = await _jwtService.GetTokenExpiryTime(accessToken);

        // Update user with refresh token and last login
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Get user roles and permissions
        var userRoles = await GetUserRoles(user.Id);
        var userPermissions = await GetUserPermissions(user.Id);

        // Prepare response
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